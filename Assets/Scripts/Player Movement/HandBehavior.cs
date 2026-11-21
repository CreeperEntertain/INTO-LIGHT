using UnityEngine;
using UnityEngine.InputSystem;

public class HandBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput inputSettings;

    [Header("Movement")]
    [SerializeField] private Vector2 offscreenOffset = new(0.5f, -0.5f);
    [SerializeField] private Vector2 handRange = new(0.3f, 0.2f);
    [SerializeField, Min(0.0001f)] private float relativeInputSensitivity = 0.003f;
    [SerializeField, Min(0.01f)] private float lerpSpeed = 120.0f;

    private bool handMode;
    private Vector2 handOffset;

    private float pivotLocalZ;
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
    }

    private void Update()
    {
        bool handModeActive = inputSettings != null && inputSettings.HandMode.IsPressed();
        if (handModeActive && !handMode)
            handOffset = Vector2.zero;
        
        handMode = handModeActive;
        Vector3 target = new(offscreenOffset.x, offscreenOffset.y, pivotLocalZ);
        if (
            handMode &&
            playerCamera != null &&
            transform.parent != null
        )
        {
            Vector2 input = inputSettings.MoveHand.Value;
            if (inputSettings.MoveHand.HasInput)
            {
                if (inputSettings.MoveHand.ActiveMode == Vector2Input.SourceMode.Relative)
                    handOffset += input * relativeInputSensitivity;
                else
                    handOffset = input;
            }
            else if (inputSettings.MoveHand.LastActiveMode != Vector2Input.SourceMode.Relative)
                handOffset = Vector2.zero;
            
            handOffset.x = Mathf.Clamp(handOffset.x, -1.0f, 1.0f);
            handOffset.y = Mathf.Clamp(handOffset.y, -1.0f, 1.0f);
            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 screenOffset = Vector2.Scale(handOffset, new(Screen.width * handRange.x, Screen.height * handRange.y));
            Vector2 screenPosition = screenCenter + screenOffset;
            Vector3 worldPosition = playerCamera.ScreenToWorldPoint(new(screenPosition.x, screenPosition.y, cameraDepth));
            target = transform.parent.InverseTransformPoint(worldPosition);
        }

        float t = 1.0f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, t);
    }
}
