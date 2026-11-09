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

    [Header("Spring")]
    public float springStrength = 100.0f;
    [Range(0.0f, 2.0f)]
    public float dampingRatio = 1.0f;
    public float jumpReleaseTime = 0.15f;

    [Header("Slope hold")]
    [Min(0f)]
    public float idleSlopeDamping = 12f;


    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector3 moveDirection;
    private float suppressSpringUntil;

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

    private struct SupportSample
    {
        public Vector3 point;
        public float weight;
    }
    private readonly SupportSample[] supportSamples = new SupportSample[12];


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

        Bounds capsuleBounds = capsule.bounds;
        float radius = Mathf.Max(capsuleBounds.extents.x, capsuleBounds.extents.z);

        Vector3 right = Vector3.ProjectOnPlane(transform.right, up).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;
        if (right.sqrMagnitude < 0.001f || forward.sqrMagnitude < 0.001f)
            return;

        float maxAngle = Mathf.Clamp(maxGroundAngle, 0.0f, 80.0f);
        float slopeRise = Mathf.Tan(maxAngle * Mathf.Deg2Rad);
        float sideOffset = radius * 0.45f;
        float farthestProbe = Mathf.Sqrt(Mathf.Pow(radius + stepProbeForward, 2.0f) + sideOffset * sideOffset);
        float castStartHeight = maxStepHeight + farthestProbe * slopeRise + 0.02f;

        int supportCount = 0;
        Vector3 normalSum = Vector3.zero;
        Vector3 pointSum = Vector3.zero;
        float totalWeight = 0.0f;

        foreach (Vector2 sample in probePattern)
        {
            Vector3 offset = probeSpread * radius * sample.x * right + probeSpread * radius * sample.y * forward;
            Vector3 sampleBase = springOrigin.position + offset;
            float maxRise = maxStepHeight + offset.magnitude * slopeRise;
            if (!TryGetSupportHit(
                sampleBase,
                up,
                castStartHeight,
                maxRise,
                out RaycastHit hit
            ))
                continue;
            
            supportSamples[supportCount++] = new SupportSample
            {
                point = hit.point,
                weight = 1.0f
            };
            pointSum += hit.point;
            normalSum += hit.normal;
            totalWeight += 1f;
        }
        if (supportCount == 0)
            return;
        
        bool hasCorePlane = TryFitSupportPlane(
            springOrigin.position,
            up,
            right,
            forward,
            supportCount,
            out Vector3 coreNormal,
            out Vector3 corePlanePoint
        );

        if (
            hasCorePlane &&
            Vector3.Angle(coreNormal, up) <= maxAngle &&
            moveDirection.sqrMagnitude > 0.001f &&
            stepProbeWeight > 0.0f
        )
        {
            Vector3 stepForward = moveDirection.normalized;
            Vector3 stepRight = Vector3.Cross(up, stepForward).normalized;
            float normalUp = Vector3.Dot(coreNormal, up);
            if (normalUp > 0.05f)
                for (int i = -1; i <= 1; i++)
                {
                    Vector3 sampleBase =
                        springOrigin.position +
                        stepForward * (radius + stepProbeForward) +
                        stepRight * (i * sideOffset);
                    Vector3 offset = Vector3.ProjectOnPlane(sampleBase - springOrigin.position, up);
                    float maxRise = maxStepHeight + offset.magnitude * slopeRise;
                    if (!TryGetSupportHit(
                        sampleBase,
                        up,
                        castStartHeight,
                        maxRise,
                        out RaycastHit hit
                    ))
                        continue;
                    
                    float expectedHeight =
                        Vector3.Dot(sampleBase, up) -
                        Vector3.Dot(sampleBase - corePlanePoint, coreNormal) / normalUp;
                    float stepRise = Vector3.Dot(hit.point, up) - expectedHeight;
                    if (stepRise <= 0.005f || stepRise > maxStepHeight)
                        continue;
                    
                    supportSamples[supportCount++] = new SupportSample
                    {
                        point = hit.point,
                        weight = stepProbeWeight
                    };
                    pointSum += hit.point * stepProbeWeight;
                    normalSum += hit.normal * stepProbeWeight;
                    totalWeight += stepProbeWeight;
                }
        }

        if (!TryFitSupportPlane(
            springOrigin.position,
            up,
            right,
            forward,
            supportCount,
            out Vector3 supportNormal,
            out Vector3 supportPlanePoint
        ))
        {
            supportNormal = normalSum.normalized;
            supportPlanePoint = pointSum / totalWeight;
        }
        if (Vector3.Dot(supportNormal, up) < 0.0f)
            supportNormal = -supportNormal;
        
        float angle = Vector3.Angle(supportNormal, up);
        if (angle > maxAngle)
        {
            Vector3 tangent = Vector3.ProjectOnPlane(supportNormal, up).normalized;
            supportNormal = (
                up * Mathf.Cos(maxAngle * Mathf.Deg2Rad) +
                tangent * Mathf.Sin(maxAngle * Mathf.Deg2Rad)
            ).normalized;
        }

        GroundNormal = supportNormal;
        Vector3 averagePoint = pointSum / totalWeight;
        float averageDistance = Vector3.Dot(springOrigin.position - supportPlanePoint, supportNormal);
        IsGrounded = averageDistance <= targetGroundGap + groundedTolerance;
        float gravityIntoSurface = Mathf.Max(0.0f, -Vector3.Dot(gravity, supportNormal));
        float naturalLength = targetGroundGap + rb.mass * gravityIntoSurface / springStrength;
        float compression = naturalLength - averageDistance;
        float normalSpeed = Vector3.Dot(rb.GetPointVelocity(averagePoint), supportNormal);
        float damping = 2.0f * dampingRatio * Mathf.Sqrt(springStrength * rb.mass);
        float force = Mathf.Max(0.0f, springStrength * compression - damping * normalSpeed);
        if (force > 0.0f)
            rb.AddForceAtPosition(
                supportNormal * force,
                averagePoint,
                ForceMode.Force
            );
        
        if (IsGrounded && moveDirection.sqrMagnitude < 0.001f)
        {
            Vector3 slopeGravity = Vector3.ProjectOnPlane(gravity, supportNormal);
            if (slopeGravity.sqrMagnitude > 0.001f)
            {
                Vector3 surfaceVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, supportNormal);
                Vector3 holdAcceleration = -slopeGravity - surfaceVelocity * idleSlopeDamping;
                rb.AddForce(holdAcceleration, ForceMode.Acceleration);
            }
        }
    }

    private bool TryFitSupportPlane(
        Vector3 origin,
        Vector3 up,
        Vector3 right,
        Vector3 forward,
        int count,
        out Vector3 normal,
        out Vector3 planePoint
    )
    {
        normal = up;
        planePoint = origin;
        if (count < 3)
            return false;
        
        float xx = 0.0f;
        float xz = 0.0f;
        float x = 0.0f;
        float zz = 0.0f;
        float z = 0.0f;
        float xy = 0.0f;
        float zy = 0.0f;
        float y = 0.0f;
        float weightSum = 0.0f;

        for (int i = 0; i < count; i++)
        {
            SupportSample sample = supportSamples[i];
            Vector3 relative = sample.point - origin;

            float px = Vector3.Dot(relative, right);
            float pz = Vector3.Dot(relative, forward);
            float py = Vector3.Dot(relative, up);
            float w = sample.weight;

            xx += px * px * w;
            xz += px * pz * w;
            x += px * w;
            zz += pz * pz * w;
            z += pz * w;

            xy += px * py * w;
            zy += pz * py * w;
            y += py * w;

            weightSum += w;
        }

        Matrix4x4 matrix = Matrix4x4.identity;

        matrix.m00 = xx;
        matrix.m01 = xz;
        matrix.m02 = x;

        matrix.m10 = xz;
        matrix.m11 = zz;
        matrix.m12 = z;

        matrix.m20 = x;
        matrix.m21 = z;
        matrix.m22 = weightSum;

        if (Mathf.Abs(matrix.determinant) < 0.000001f)
            return false;
        
        Vector4 fit = matrix.inverse * new Vector4(xy, zy, y, 0f);
        normal = (up - right * fit.x - forward * fit.y).normalized;
        if (Vector3.Dot(normal, up) < 0.0f)
            normal = -normal;
        
        planePoint = origin + up * fit.z;
        return true;
    }

    private bool TryGetSupportHit(
        Vector3 sampleBase,
        Vector3 up,
        float startHeight,
        float maxRise,
        out RaycastHit bestHit
    )
    {
        bestHit = default;
        
        Vector3 rayOrigin = sampleBase + up * startHeight;
        float rayLength = startHeight + maxLength + 0.02f;
        int hitCount = Physics.RaycastNonAlloc(
            rayOrigin,
            -up,
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
            
            float separation = Vector3.Dot(sampleBase - hit.point, hit.normal);
            if (separation < -maxRise || separation > maxLength)
                continue;
            if (hit.distance >= closestHitDistance)
                continue;
            
            closestHitDistance = hit.distance;
            bestHit = hit;
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
