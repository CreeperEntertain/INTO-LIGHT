using System;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class GroundSpring : MonoBehaviour
{
    [Header("Ground probe")]
    public Transform springOrigin;
    public LayerMask groundMask;
    public float targetGroundGap = 1f / 3f;
    public float maxLength = 2f / 3f;
    public float maxGroundAngle = 50f;

    [Header("Support probes")]
    [Range(0.1f, 1.0f)]
    public float probeSpread = 0.75f;
    [Min(0.0f)]
    public float maxStepHeight = 0.2f;
    [Min(0.0f)]
    public float stepProbeForward = 0.08f;
    [Min(0.0f)]
    public float stepProbeWeight = 1.0f;
    [Min(0.0f)]
    public float groundedTolerance = 0.08f;
    [Min(0.0f)]
    public float normalMemoryTime = 0.2f;

    [Header("Spring")]
    public float springStrength = 100.0f;
    [Range(0.0f, 2.0f)]
    public float dampingRatio = 1.0f;
    public float jumpReleaseTime = 0.15f;


    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector3 moveDirection;
    private float suppressSpringUntil;
    private float lastSupportTime = float.NegativeInfinity;

    private readonly RaycastHit[] hitBuffer = new RaycastHit[32];
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


    public bool IsGrounded {get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }

    public void SetMoveDirection(Vector3 direction)
    {
        Vector3 up = Physics.gravity.sqrMagnitude > 0.0001f
            ? -Physics.gravity.normalized
            : Vector3.up;
        moveDirection = Vector3.ProjectOnPlane(direction, up);
        if (moveDirection.sqrMagnitude > 0.0001f)
            moveDirection.Normalize();
    }

    public void ReleaseForJump()
    {
        suppressSpringUntil = Time.fixedTime + jumpReleaseTime;
        IsGrounded = false;
    }

    private void FixedUpdate()
    {
        IsGrounded = false;
        if (springOrigin == null || springStrength <= 0.0f)
            return;
        if (Time.fixedTime < suppressSpringUntil)
            return;
        
        Vector3 gravity = Physics.gravity;
        Vector3 up = gravity.sqrMagnitude > 0.0001f
            ? -gravity.normalized : Vector3.up;
        Vector3 castDirection = Time.fixedTime - lastSupportTime <= normalMemoryTime
            ? -GroundNormal : -up;
        
        castDirection.Normalize();

        Bounds capsuleBounds = capsule.bounds;
        float radius = Mathf.Max(capsuleBounds.extents.x, capsuleBounds.extents.z);

        float castStartHeight = maxStepHeight + 0.02f;

        Vector3 right = Vector3.ProjectOnPlane(transform.right, up).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;
        if (right.sqrMagnitude < 0.001f || forward.sqrMagnitude < 0.001f)
            return;

        float distanceSum = 0.0f;
        Vector3 normalSum = Vector3.zero;
        Vector3 pointSum = Vector3.zero;
        int supportCount = 0;

        foreach (Vector2 sample in probePattern)
        {
            Vector3 offset = probeSpread * radius * sample.x * right + probeSpread * radius * sample.y * forward;
            Vector3 sampleBase = springOrigin.position + offset;
            if (!TryGetSupportHit(
                sampleBase,
                up,
                castDirection,
                castStartHeight,
                out RaycastHit hit,
                out float separation
            ))
                continue;
            
            distanceSum += separation;
            normalSum += hit.normal;
            pointSum += hit.point;
            supportCount++;
        }
        if (supportCount == 0)
            return;

        float coreAverageDistance = distanceSum / supportCount;
        float totalWeight = supportCount;

        if (
            moveDirection.sqrMagnitude > 0.001f &&
            maxStepHeight > 0.0f &&
            stepProbeWeight > 0.0f
        )
        {
            Vector3 stepForward = Vector3.ProjectOnPlane(moveDirection, up).normalized;
            Vector3 stepRight = Vector3.Cross(up, stepForward).normalized;
            for (int i = -1; i <= 1; i++)
            {
                Vector3 sampleBase =
                    springOrigin.position +
                    stepForward * (radius + stepProbeForward) +
                    stepRight * (i * radius * 0.45f);
                if (!TryGetSupportHit(
                    sampleBase,
                    up,
                    castDirection,
                    castStartHeight,
                    out RaycastHit hit,
                    out float separation
                ))
                    continue;
                
                float rise = coreAverageDistance - separation;
                if (rise <= 0.005f || rise > maxStepHeight)
                    continue;
                
                distanceSum += separation * stepProbeWeight;
                normalSum += hit.normal * stepProbeWeight;
                pointSum += hit.point * stepProbeWeight;
                totalWeight += stepProbeWeight;
            }
        }

        float averageDistance = distanceSum / totalWeight;
        Vector3 averageNormal = normalSum.normalized;
        Vector3 averagePoint = pointSum / totalWeight;

        GroundNormal = averageNormal;
        lastSupportTime = Time.fixedTime;
        IsGrounded = averageDistance <= targetGroundGap + groundedTolerance;

        float gravityIntoSurface = Mathf.Max(0.0f, -Vector3.Dot(gravity, averageNormal));
        float naturalLength = targetGroundGap + rb.mass * gravityIntoSurface / springStrength;
        float compression = naturalLength - averageDistance;
        float normalSpeed = Vector3.Dot(rb.GetPointVelocity(averagePoint), averageNormal);
        float damping = 2.0f * dampingRatio * Mathf.Sqrt(springStrength * rb.mass);
        float force = springStrength * compression - damping * normalSpeed;
        force = Mathf.Max(0.0f, force);
        if (force > 0.0f)
            rb.AddForceAtPosition(
                averageNormal * force,
                averagePoint,
                ForceMode.Force
            );
    }

    private bool TryGetSupportHit(
        Vector3 sampleBase,
        Vector3 up,
        Vector3 castDirection,
        float startHeight,
        out RaycastHit bestHit,
        out float separation
    )
    {
        bestHit = default;
        separation = 0.0f;
        float downwardComponent = Vector3.Dot(up, -castDirection);
        if (downwardComponent <= 0.01f)
            return false;
        
        Vector3 rayOrigin = sampleBase + up * startHeight;
        float rayLength = (startHeight + maxLength) / downwardComponent + 0.02f;
        int hitCount = Physics.RaycastNonAlloc(
            rayOrigin,
            castDirection,
            hitBuffer,
            rayLength,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        float closestHitDistance = float.PositiveInfinity;
        bool found = false;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitBuffer[i];
            if (hit.collider == null)
                continue;
            if (
                hit.collider.attachedRigidbody == rb ||
                hit.transform == transform ||
                hit.transform.IsChildOf(transform)
            )
                continue;
            if (Vector3.Angle(hit.normal, up) > maxGroundAngle)
                continue;
            
            float candidateSeparation = Vector3.Dot(sampleBase - hit.point, hit.normal);
            if (candidateSeparation < -maxStepHeight || candidateSeparation > maxLength)
                continue;
            if (hit.distance >= closestHitDistance)
                continue;
            
            closestHitDistance = hit.distance;
            bestHit = hit;
            separation = candidateSeparation;
            found = true;
        }

        return found;
    }

    private void OGizmosSelected()
    {
        if (springOrigin == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(springOrigin.position, 0.04f);
    }
}
