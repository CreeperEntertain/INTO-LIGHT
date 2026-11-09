using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(GroundSpring))]
public class MovementController : MonoBehaviour
{
    [Header("References")]
    public Transform yawPivot;
    public Transform viewPivot;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 80f;
    public float braking = 80f;
    public float velocityResponse = 10f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.08f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Jump")]
    public float jumpImpulse = 5.5f;

    private Rigidbody rb;
    private GroundSpring groundSpring;
    private WallContactSmoother wallContactSmoother;

    private Vector2 moveInput;

    private float yaw;
    private float pitch;

    private bool jumpQueued;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundSpring = GetComponent<GroundSpring>();
        wallContactSmoother = GetComponent<WallContactSmoother>();

        rb.constraints |= RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (yawPivot != null)
            yawPivot.localRotation = Quaternion.identity;
        if (viewPivot != null)
            viewPivot.localRotation = Quaternion.identity;
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
        {
            moveInput = Vector2.zero;
            return;
        }
        
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
        if (yawPivot != null)
            yawPivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        if (viewPivot != null)
            viewPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }


    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.y) > 0.3f)
                continue;
            Vector3 normal = Vector3.ProjectOnPlane(contact.normal, Vector3.up);
            if (normal.sqrMagnitude < 0.001f)
                continue;
        }
    }

    private void HandleMovement()
    {
        Transform movementFrame = yawPivot != null ? yawPivot : transform;

        Vector3 forward = Vector3.ProjectOnPlane(movementFrame.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(movementFrame.right, Vector3.up).normalized;

        Vector3 direction = right * moveInput.x + forward * moveInput.y;
        direction = Vector3.ClampMagnitude(direction, 1.0f);

        Vector3 desitedVelocity = direction * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0.0f;

        Vector3 velocityError = desitedVelocity - currentVelocity;
        float maxAcceleration = moveInput.sqrMagnitude > 0.001f
            ? acceleration : braking;
        
        Vector3 accelerationVector = Vector3.ClampMagnitude(velocityError * velocityResponse, maxAcceleration);

        if (wallContactSmoother.TryGetWallNormal(out Vector3 wallNormal))
        {
            float intoWall = Vector3.Dot(accelerationVector, wallNormal);
            if (intoWall < 0.0f)
            accelerationVector -= wallNormal * intoWall;
        }

        rb.AddForce(accelerationVector, ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        if (!jumpQueued)
            return;
        
        jumpQueued = false;
        if (!groundSpring.IsGrounded)
            return;
        
        groundSpring.ReleaseForJump();
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
