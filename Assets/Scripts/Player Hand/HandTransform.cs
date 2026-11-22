using UnityEngine;

public class HandTransform : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private Quaternion rotationOffset;
    private HandBehavior handBehavior;


    private void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("HandTransform: Assign the camera Transform in the Inspector.", this);
            enabled = false;
            return;
        }

        handBehavior = GetComponentInParent<HandBehavior>();
        Vector3 directionToCamera = cameraTransform.position - transform.position;
        if (directionToCamera.sqrMagnitude < 1.0e-6f)
        {
            Debug.LogError("HandTransform: The hand and camera cannot share a position.", this);
            enabled = false;
            return;
        }
        
        Quaternion initialLookRotation = Quaternion.LookRotation(directionToCamera, cameraTransform.up);
        rotationOffset = Quaternion.Inverse(initialLookRotation) * transform.rotation;
    }

    private void LateUpdate()
    {
        Vector3 directionToCamera = cameraTransform.position - transform.position;
        if (directionToCamera.sqrMagnitude < 1.0e-6f)
            return;
        
        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera, cameraTransform.up);
        Quaternion correctedRotation = lookRotation * rotationOffset;
        float roll = handBehavior != null ? handBehavior.MoveHandInput.z : 0.0f;
        transform.rotation = correctedRotation * Quaternion.AngleAxis(roll, Vector3.forward);
    }
}
