using UnityEngine;

[DefaultExecutionOrder(300)]
[RequireComponent(typeof(Rigidbody))]
public class GroundMotionTransfer : MonoBehaviour
{
    [Header("Platform Carry")]
    [Min(0.0f)]
    public float collisionSkin = 0.002f;
    [Min(1)]
    public int maxSlideIterations = 4;


    private Rigidbody body;
    private Transform support;
    private Rigidbody supportBody;

    private Matrix4x4 previousSupportMatrix;
    private bool hasPreviousSample;


    private void Awake()
        => body = GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        if (support == null)
        {
            hasPreviousSample = false;
            return;
        }

        Matrix4x4 currentMatrix = support.localToWorldMatrix;
        if (!hasPreviousSample)
        {
            previousSupportMatrix = currentMatrix;
            hasPreviousSample = true;
            return;
        }

        Matrix4x4 delta = currentMatrix * previousSupportMatrix.inverse;
        previousSupportMatrix = currentMatrix;
        Vector3 targetPosition = delta.MultiplyPoint3x4(body.position);
        Vector3 displacement = targetPosition - body.position;
        if (displacement.sqrMagnitude < 1.0e-10f)
            MoveCollision(displacement);
    }

    private void OnCollisionStay(Collision collision)
    {
        Transform candidate = null;
        Rigidbody candidateBody = null;

        foreach(ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) < 0.5f)
                continue;
            
            Collider other = contact.otherCollider;
            if (other == null)
                continue;
            
            candidateBody = other.attachedRigidbody;
            candidate = candidateBody != null
                ? candidateBody.transform : other.transform;
            
            break;
        }
        if (candidate == null)
            return;
        if (support != candidate)
        {
            support = candidate;
            supportBody = candidateBody;
            hasPreviousSample = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (support == null)
            return;
        
        Transform other = collision.transform;
        if (
            other == support ||
            other.IsChildOf(support) ||
            (support != null && collision.rigidbody == supportBody)
        )
        {
            support = null;
            supportBody = null;
            hasPreviousSample = false;
        }
    }


    private void MoveCollision(Vector3 displacement)
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
                if (hit.collider == null)
                    continue;
                if (IsSupportCollider(hit.collider))
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

    private bool IsSupportCollider(Collider collider)
    {
        if (supportBody != null && collider.attachedRigidbody == supportBody)
            return true;
        
        Transform t = collider.transform;
        return t == support || t.IsChildOf(support);
    }
}
