using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("References")]
    public Transform viewPivot;
    public Transform springOrigin;
    public LayerMask groundMask;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 30f;
    public float braking = 40f;
    public float velocityResponse = 10f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.08f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Jump")]
    public float jumpImpulse = 5.5f;
    public float groundedDistance = 0.12f;

    private Rigidbody rb;
    private Vector2 moveInput;

    private float yaw;
    private float pitch;

    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |=
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        yaw = rb.rotation.eulerAngles.y;
    }

    private void Start()
        => LockCursor();
    
    private void Update()
    {
        ReadMovementInput();
        ReadMouseInput();
    }

    private void ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        
        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1.0f;
        if (keyboard.sKey.isPressed) input.y -= 1.0f;
        if (keyboard.dKey.isPressed) input.x += 1.0f;
        if (keyboard.aKey.isPressed) input.x -= 1.0f;

        moveInput = Vector2.ClampMagnitude(input, 1.0f);
        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpQueued = true;
        if (keyboard.escapeKey.wasPressedThisFrame)
            UnlockCursor();
    }

    private void ReadMouseInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;
        if (mouse.leftButton.wasPressedThisFrame)
            LockCursor();
        if (Cursor.lockState != CursorLockMode.Locked)
            return;
        
        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * mouseSensitivity;

        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (viewPivot != null)
            viewPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }


    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
        HandleJump();
    }

    private void HandleRotation()
        => rb.MoveRotation(Quaternion.Euler(0.0f, yaw, 0.0f));
    
    private void HandleMovement()
    {
        Quaternion yawRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        Vector3 desiredVelocity = yawRotation * new Vector3(moveInput.x, 0.0f, moveInput.y) * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0.0f;
        Vector3 velocityError = desiredVelocity - currentVelocity;

        float maxAcceleration = moveInput.sqrMagnitude > 0.001f
            ? acceleration
            : braking;
        Vector3 accelerationVector = Vector3.ClampMagnitude(
            velocityError * velocityResponse,
            maxAcceleration
        );

        rb.AddForce(accelerationVector, ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        if (!jumpQueued)
            return;
        
        jumpQueued = false;
        if (springOrigin == null)
            return;
        
        bool grounded = Physics.Raycast(
            springOrigin.position,
            Vector3.down,
            out _,
            groundedDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        if (grounded)
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }


    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
