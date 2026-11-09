using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody))]
public class GroundSpring : MonoBehaviour
{
    [Header("Ground probe")]
    public Transform springOrigin;
    public LayerMask groundMask;
    public float targetGroundGap = 1f / 3f;
    public float maxLength = 2f / 3f;
    public float maxGroundAngle = 50f;

    [Header("Spring")]
    public float springStrength = 100f;
    [Range(0f, 2f)]
    public float dampingRatio = 1f;
    public float jumpReleaseTime = 0.15f;

    private Rigidbody rb;
    private float suppressSpringUntil;

    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    private void Awake()
        => rb = GetComponent<Rigidbody>();

    public void ReleaseForJump()
    {
        suppressSpringUntil = Time.fixedTime + jumpReleaseTime;
        IsGrounded = false;
    }

    private void FixedUpdate()
    {
        IsGrounded = false;
        if (springOrigin == null || springStrength <= 0f)
            return;
        if (Time.fixedTime < suppressSpringUntil)
            return;

        bool foundSurface = Physics.Raycast(
            springOrigin.position,
            Vector3.down,
            out RaycastHit hit,
            maxLength,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        Debug.DrawRay(
            springOrigin.position,
            Vector3.down * maxLength,
            foundSurface ? Color.green : Color.red
        );
        if (!foundSurface)
            return;
        if (Vector3.Angle(hit.normal, Vector3.up) > maxGroundAngle)
            return;
        
        GroundNormal = hit.normal;
        float gravityIntoSurface = Mathf.Max(0f, -Vector3.Dot(Physics.gravity, hit.normal));
        float naturalLength = targetGroundGap + rb.mass * gravityIntoSurface / springStrength;
        float compression = naturalLength - hit.distance;
        float normalSpeed = Vector3.Dot(rb.GetPointVelocity(springOrigin.position), hit.normal);
        float damping = 2.0f * dampingRatio * Mathf.Sqrt(springStrength * rb.mass);
        float force = springStrength * compression - damping * normalSpeed;
        force = Mathf.Max(0f, force);
        if (force <= 0.0f)
            return;
        
        IsGrounded = true;
        rb.AddForceAtPosition(
            hit.normal * force,
            springOrigin.position,
            ForceMode.Force
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (springOrigin == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            springOrigin.position,
            springOrigin.position + Vector3.down * maxLength
        );
    }
}
