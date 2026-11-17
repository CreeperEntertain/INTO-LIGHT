using UnityEngine;
using UnityEngine.InputSystem;

public class HandBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private Vector2 offscreenOffset = new(-1.5f, -1.0f);
    [SerializeField, Min(0.01f)] private float lerpSpeed = 120.0f;

    private bool handMode;
    private CursorLockMode previousLockState;

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
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool shiftHeld = keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        bool justEnteredHandMode = false;
        if (shiftHeld && !handMode)
        {
            previousLockState = Cursor.lockState;

            Cursor.lockState = CursorLockMode.None;
            if (mouse != null)
            {
                Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
                mouse.WarpCursorPosition(center);
            }

            handMode = true;
            justEnteredHandMode = true;
        }
        else if (!shiftHeld && handMode)
        {
            handMode = false;
            Cursor.lockState = previousLockState;
        }

        Vector3 target = new(offscreenOffset.x, offscreenOffset.y, pivotLocalZ);
        if (
            handMode &&
            mouse != null &&
            playerCamera != null &&
            transform.parent != null
        )
        {
            Vector2 mousePosition = justEnteredHandMode
                ? new(Screen.width * 0.5f, Screen.height * 0.5f) : mouse.position.ReadValue();
            Vector3 screenPosition = new(mousePosition.x, mousePosition.y, cameraDepth);
            Vector3 worldPosition = playerCamera.ScreenToWorldPoint(screenPosition);
            target = transform.parent.InverseTransformPoint(worldPosition);
        }

        float t = 1.0f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, t);
    }
}
