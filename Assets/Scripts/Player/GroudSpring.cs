using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroudSpring : MonoBehaviour
{
    [Header("Ground probe")]
    public Transform springOrigin;
    public LayerMask groundMask = ~0;
    public float restLength = 0.3f;
    public float maxLength = 0.55f;

    [Header("Spring")]
    public float springStrength = 2500f;
    public float damping = 180f;

    Rigidbody rb;

    private void Awake()
        => rb = GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        if (springOrigin == null)
            return;
        if (!Physics.Raycast(
            new(springOrigin.position, Vector3.down),
            out RaycastHit hit,
            maxLength,
            groundMask,
            QueryTriggerInteraction.Ignore))
            return;
        
        float compression = restLength - hit.distance;
        float normalSpeed = Vector3.Dot(
            rb.GetPointVelocity(hit.point),
            hit.normal
        );
        float force = springStrength * compression - damping * normalSpeed;
        force = Mathf.Max(0.0f, force);
        if (force > 0.0f)
            rb.AddForceAtPosition(
                hit.normal * force,
                hit.point,
                ForceMode.Force
            );
    }

    private void OwGizmosSelected()
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
