using UnityEngine;

public class HandBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput inputSettings;

    [Header("Movement")]
    [SerializeField] private Vector2 offscreenOffset = new(0.5f, -0.5f);
    [SerializeField, Min(0.0001f)] private float relativeInputSensitivity = 1.0f;
    [SerializeField, Min(0.01f)] private float lerpSpeed = 120.0f;

    private bool handMode;
    private Vector2 handOffset;

    private float pivotLocalZ;
    private float handRoll;
    private float cameraDepth;


    private void Awake()
    {
        if (playerCamera == null && transform.parent != null)
            playerCamera = transform.parent.GetComponentInChildren<Camera>();
        
        pivotLocalZ = transform.localPosition.z;
        if (playerCamera != null)
            cameraDepth = Vector3.Dot(
                transform.position - playerCamera.transform.position,
                playerCamera.transform.forward
            );
        else
            Debug.LogError("HandBehavior requires a camera.", this);
        
        handRoll = transform.localEulerAngles.z;
    }

    private void Update()
    {
        bool handModeActive = inputSettings != null && inputSettings.HandMode.IsPressed();
        if (handModeActive && !handMode)
            handOffset = Vector2.zero;
        
        handMode = handModeActive;

        Vector3 input = inputSettings.MoveHand.Value;
        bool hasInput = inputSettings.MoveHand.HasInput;
        Vector3Input.SourceMode? activeMode = inputSettings.MoveHand.ActiveMode;
        if (hasInput)
            handRoll = input.z;

        Vector3 target = new(offscreenOffset.x, offscreenOffset.y, pivotLocalZ);
        if (
            handMode &&
            playerCamera != null &&
            transform.parent != null
        )
        {
            if (inputSettings.MoveHand.HasInput)
            {
                if (activeMode == Vector3Input.SourceMode.Relative)
                    handOffset += new Vector2(
                        input.x * 2.0f / Mathf.Max(1, Screen.width),
                        input.y * 2.0f / Mathf.Max(1, Screen.height)
                    ) * relativeInputSensitivity;
                else
                    handOffset = input;
            }
            else if (inputSettings.MoveHand.LastActiveMode != Vector3Input.SourceMode.Relative)
                handOffset = Vector2.zero;
            
            handOffset.x = Mathf.Clamp(handOffset.x, -1.0f, 1.0f);
            handOffset.y = Mathf.Clamp(handOffset.y, -1.0f, 1.0f);
            Vector2 screenPosition =  new(
                (handOffset.x + 1.0f) * 0.5f * Screen.width,
                (handOffset.y + 1.0f) * 0.5f * Screen.height
            );
            Vector3 worldPosition = playerCamera.ScreenToWorldPoint(new(screenPosition.x, screenPosition.y, cameraDepth));
            target = transform.parent.InverseTransformPoint(worldPosition);
        }

        transform.localRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            handRoll
        );
        float t = 1.0f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, t);
    }
}
