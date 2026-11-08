using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroudSpring : MonoBehaviour
{
    [Header("Ground probe")]
    public Transform springOrigin;
    public LayerMask groundMask;
    public float targetGroundGap = 0.02f;
    public float maxLength = 0.55f;

    [Header("Spring")]
    public float springStrength = 2500f;
    [Range(0f, 2f)]
    public float dampingRatio = 1f;

    private Rigidbody rb;

    private void Awake()
     => rb = GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        if (springOrigin == null || springStrength <= 0f)
            return;

        bool grounded = Physics.Raycast(
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
            grounded ? Color.green : Color.red
        );
        if (!grounded)
            return;

        float gravityIntoSurface = Mathf.Max(0f, -Vector3.Dot(Physics.gravity, hit.normal));
        float naturalLength = targetGroundGap + rb.mass * gravityIntoSurface / springStrength;
        float compression = naturalLength - hit.distance;
        float normalSpeed = Vector3.Dot(
            rb.GetPointVelocity(springOrigin.position),
            hit.normal
        );
        float damping = 2f * dampingRatio * Mathf.Sqrt(springStrength * rb.mass);
        float force =springStrength * compression - damping * normalSpeed;

        force = Mathf.Max(0f, force);

        if (force > 0f)
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
