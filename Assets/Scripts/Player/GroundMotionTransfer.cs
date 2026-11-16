using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(GroundSpring))]
public class GroundMotionTransfer : MonoBehaviour
{
    [Header("Collision-safe carry")]
    [Min(0.0f)]
    public float collisionSkin = 0.002f;
    [Min(1)]
    public int maxSlideIterations = 4;

    private Rigidbody body;
    private CapsuleCollider capsule;
    private GroundSpring spring;

    private Transform support;
    private Matrix4x4 previousSupportMatrix;
    private bool hasPreviousSample;
    private Vector3 supportVelocity;
    private Vector3 detachedSupportVelocity;
    private bool hasDetachedSupportVelocity;

    private readonly RaycastHit[] hitBuffer = new RaycastHit[32];
    private float lastSupportMotionTime;

    private static readonly Vector2[] probePattern =
    {
        new(0.0f, 0.0f),
        new(1.0f, 0.0f),
        new(-1.0f, 0.0f),
        new(0.0f, 1.0f),
        new(0.0f, -1.0f),
        new(0.7071f, 0.7071f),
        new(-0.7071f, 0.7071f),
        new(0.7071f, -0.7071f),
        new(-0.7071f, -0.7071f)
    };


    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        spring = GetComponent<GroundSpring>();
    }

    private void Update()
    {
        Physics.SyncTransforms();
        if (!spring.IsGrounded || !TryFindSupport(out Collider supportCollider))
        {
            DetachAndTransferVelocity();
            return;
        }

        Transform candidate = ResolveSupportTransform(supportCollider);
        if (candidate != support && hasPreviousSample)
            DetachAndTransferVelocity();
        
        hasDetachedSupportVelocity = false;
        detachedSupportVelocity = Vector3.zero;

        if (candidate != support || !hasPreviousSample)
        {
            hasDetachedSupportVelocity = false;
            detachedSupportVelocity = Vector3.zero;

            support = candidate;
            previousSupportMatrix = candidate.localToWorldMatrix;
            hasPreviousSample = true;
            supportVelocity = Vector3.zero;
            lastSupportMotionTime = Time.time;
            return;
        }

        Matrix4x4 currentMatrix = support.localToWorldMatrix;
        if (!HasMatrixChanged(currentMatrix, previousSupportMatrix))
        {
            if (Time.time - lastSupportMotionTime > Time.fixedDeltaTime * 2.5f)
                supportVelocity = Vector3.zero;
            return;
        }

        Matrix4x4 delta = currentMatrix * previousSupportMatrix.inverse;
        Vector3 targetPosition = delta.MultiplyPoint3x4(body.position);
        Vector3 displacement = targetPosition - body.position;
        float elapsed = Time.time - lastSupportMotionTime;
        previousSupportMatrix = currentMatrix;
        lastSupportMotionTime = Time.time;
        if (displacement.sqrMagnitude <= 1.0e-10f)
        {
            supportVelocity = Vector3.zero;
            return;
        }

        Vector3 positionBeforeCarry = body.position;
        MoveCollisionSafe(displacement);
        Vector3 appliedDisplacement = body.position - positionBeforeCarry;
        supportVelocity = elapsed > 1.0e-5f
            ? appliedDisplacement / elapsed : Vector3.zero;
    }

    private Transform ResolveSupportTransform(Collider supportCollider)
    {
        Rigidbody supportBody = supportCollider.attachedRigidbody;
        if (supportBody != null)
            return supportBody.transform;
        
        Animator animator = supportCollider.GetComponentInParent<Animator>();
        return animator != null
            ? animator.transform : supportCollider.transform;
    }

    private static bool HasMatrixChanged(Matrix4x4 current, Matrix4x4 previous)
    {
        for (int i = 0; i < 16; i++)
            if (Mathf.Abs(current[i] - previous[i]) > 1.0e-6f)
                return true;
        return false;
    }

    private bool TryFindSupport(out Collider bestCollider)
    {
        bestCollider = null;
        if (spring.springOrigin == null)
            return false;

        Vector3 gravity = Physics.gravity;
        Vector3 up = gravity.sqrMagnitude > 0.0001f
            ? -gravity.normalized : Vector3.up;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, up).normalized;
        if (right.sqrMagnitude < 0.001f)
            return false;

        Vector3 forward = Vector3.Cross(right, up).normalized;
        if (forward.sqrMagnitude < 0.001f)
            return false;

        Bounds capsuleBounds = capsule.bounds;
        float radius = Mathf.Max(capsuleBounds.extents.x, capsuleBounds.extents.z);
        float maxAngle = Mathf.Clamp(spring.maxGroundAngle, 0.0f, 80.0f);
        float slopeRise = Mathf.Tan(maxAngle * Mathf.Deg2Rad);
        float sideOffset = radius * 0.45f;
        float farthestProbe = Mathf.Sqrt(
            Mathf.Pow(radius + spring.stepProbeForward, 2.0f) +
            sideOffset * sideOffset
        );
        float castStartHeight = spring.maxStepHeight + farthestProbe * slopeRise + 0.02f;

        foreach (Vector2 sample in probePattern)
        {
            Vector3 offset =
                spring.probeSpread * radius * sample.x * right +
                spring.probeSpread * radius * sample.y * forward;
            Vector3 sampleBase = spring.springOrigin.position + offset;
            float maxRise = spring.maxStepHeight + offset.magnitude * slopeRise;
            Vector3 rayOrigin = sampleBase + up * castStartHeight;
            float rayLength = castStartHeight + spring.maxLength + 0.02f;
            int hitCount = Physics.RaycastNonAlloc(
                rayOrigin,
                -up,
                hitBuffer,
                rayLength,
                spring.groundMask,
                QueryTriggerInteraction.Ignore
            );

            float closestDistance = float.PositiveInfinity;
            Collider sampleBestCollider = null;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Collider other = hit.collider;
                if (other == null)
                    continue;
                if (other.attachedRigidbody == body ||
                    other.transform == transform ||
                    other.transform.IsChildOf(transform)
                )
                    continue;
                if (Vector3.Angle(hit.normal, up) > spring.maxGroundAngle)
                    continue;

                float separation = Vector3.Dot(sampleBase - hit.point, hit.normal);
                if (separation < -maxRise || separation > spring.maxLength)
                    continue;
                if (hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                sampleBestCollider = other;
            }
            if (sampleBestCollider != null)
            {
                bestCollider = sampleBestCollider;
                return true;
            }
        }

        return false;
    }

    private void MoveCollisionSafe(Vector3 displacement)
    {
        Vector3 remaining = displacement;
        for (int i = 0; i < maxSlideIterations; i++)
        {
            float distance = remaining.magnitude;
            if (distance < 1.0e-5f)
                break;
            
            Vector3 direction = remaining / distance;
            RaycastHit[] hits = body.SweepTestAll(direction, distance, QueryTriggerInteraction.Ignore);
            bool blocked = false;
            RaycastHit closestHit = default;
            float closestDistance = distance;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || IsSupportCollider(hit.collider))
                    continue;
                if (hit.distance >= closestDistance)
                    continue;
                
                closestDistance = hit.distance;
                closestHit = hit;
                blocked = true;
            }
            if (!blocked)
            {
                body.position += remaining;
                break;
            }

            float allowedDistance = Mathf.Max(0.0f, closestDistance - collisionSkin);
            Vector3 allowed = direction * allowedDistance;

            body.position += allowed;
            remaining -= allowed;
            if (closestHit.normal.sqrMagnitude < 1.0e-6f)
                break;
            if (Vector3.Dot(remaining, closestHit.normal) >= 0.0f)
                break;
            
            remaining = Vector3.ProjectOnPlane(remaining, closestHit.normal);
        }
    }

    private void DetachAndTransferVelocity()
    {
        if (!hasPreviousSample)
            return;
        
        detachedSupportVelocity = supportVelocity;
        hasDetachedSupportVelocity = true;

        body.linearVelocity += supportVelocity;

        ClearSupport();
    }

    private void ClearSupport()
    {
        support = null;
        hasPreviousSample = false;
        supportVelocity = Vector3.zero;
    }

    public Vector3 ReleaseSupportVelocityForJump()
    {
        if (hasPreviousSample)
        {
            Vector3 inheritedVelocity = supportVelocity;
            body.linearVelocity += inheritedVelocity;

            ClearSupport();

            detachedSupportVelocity = Vector3.zero;
            hasDetachedSupportVelocity = false;

            return inheritedVelocity;
        }

        Vector3 pendingVelocity = hasDetachedSupportVelocity
            ? detachedSupportVelocity : Vector3.zero;

        detachedSupportVelocity = Vector3.zero;
        hasDetachedSupportVelocity = false;

        return pendingVelocity;
    }



    private bool IsSupportCollider(Collider other)
    {
        if (other == null || support == null)
            return false;
        Transform t = other.transform;
        return t == support || t.IsChildOf(support);
    }
}