using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(GroundSpring))]
public class MovementController : MonoBehaviour
{
    [Header("References")]
    public Transform yawPivot;
    public Transform viewPivot;
    public PlayerInput inputSettings;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float acceleration = 80.0f;
    [Range(0.0f, 100.0f)]
    public float airAcceleratonPercentage = 15.0f;
    public float braking = 80.0f;
    public float velocityResponse = 10.0f;

    [Header("Camera Look")]
    public float mouseSensitivity = 0.08f;
    public float gamepadLookSensitivity = 90.0f;
    public float minPitch = -90.0f;
    public float maxPitch = 90.0f;

    [Header("Jump")]
    [Min(0.0f)]
    public float coyoteTime = 0.25f;
    [Min(0.0f)]
    public float jumpSpeed = 5.5f;
    [Min(0.0f)]
    public float jumpBufferTime = 0.2f;

    private Rigidbody rb;
    private GroundSpring groundSpring;
    private WallContactSmoother wallContactSmoother;
    private GroundMotionTransfer groundMotionTransfer;

    private Vector2 moveInput;

    private float yaw;
    private float pitch;

    private float lastGroundedTime = float.NegativeInfinity;
    private float jumpPressedTime = float.NegativeInfinity;
    private bool jumpQueued;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundSpring = GetComponent<GroundSpring>();
        wallContactSmoother = GetComponent<WallContactSmoother>();
        groundMotionTransfer = GetComponent<GroundMotionTransfer>();

        rb.constraints |= RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.None;
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
        ReadCameraInput();
    }

    private void ReadMovementInput()
    {
        if (inputSettings == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = inputSettings.Move.Value;
        if (inputSettings.Jump.WasPressedThisFrame())
        {
            jumpQueued = true;
            jumpPressedTime = Time.fixedTime;
        }
    }

    private void ReadCameraInput()
    {
        if (inputSettings == null || inputSettings.HandMode.IsPressed())
            return;
        
        Vector2 lookInput = inputSettings.MoveCamera.Value;
        if (lookInput.sqrMagnitude < 0.0001f)
            return;
        
        bool usingMouse = inputSettings.MoveCamera.IsRelative;
        if (usingMouse)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;
            yaw += lookInput.x * mouseSensitivity;
            pitch -= lookInput.y * mouseSensitivity;
        }
        else
        {
            yaw += lookInput.x * gamepadLookSensitivity * Time.deltaTime;
            pitch -= lookInput.y * gamepadLookSensitivity * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (yawPivot != null)
            yawPivot.localRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        if (viewPivot != null)
            viewPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }


    private void FixedUpdate()
    {
        if (groundSpring.IsGrounded)
            lastGroundedTime = Time.fixedTime;
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

        groundSpring.SetMoveDirection(direction);
        if (moveInput.sqrMagnitude < 0.001f && !groundSpring.IsGrounded)
            return;

        Vector3 desiredVelocity = direction * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        currentVelocity.y = 0.0f;
        Vector3 velocityError = desiredVelocity - currentVelocity;
        float maxAcceleration = moveInput.sqrMagnitude > 0.001f
            ? acceleration * (groundSpring.IsGrounded
                ? 1.0f : airAcceleratonPercentage / 100.0f
            ) : braking;
        
        Vector3 accelerationVector = Vector3.ClampMagnitude(velocityError * velocityResponse, maxAcceleration);

        if (wallContactSmoother.TryGetWallNormal(out Vector3 wallNormal2))
        {
            float intoWall = Vector3.Dot(accelerationVector, wallNormal2);
            if (intoWall < 0.0f)
                accelerationVector -= wallNormal2 * intoWall;
        }

        rb.AddForce(accelerationVector, ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        if (!jumpQueued)
            return;
        if (Time.fixedTime - jumpPressedTime > jumpBufferTime)
        {
            jumpQueued = false;
            return;
        }
        
        bool canJump = Time.fixedTime - lastGroundedTime <= coyoteTime;
        if (!canJump)
            return;

        jumpQueued = false;

        Vector3 inheritedVelocity = groundMotionTransfer != null
            ? groundMotionTransfer.ReleaseSupportVelocityForJump() : Vector3.zero;

        groundSpring.ReleaseForJump();

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpSpeed + inheritedVelocity.y;
        rb.linearVelocity = velocity;

        lastGroundedTime = float.NegativeInfinity;
    }


    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
