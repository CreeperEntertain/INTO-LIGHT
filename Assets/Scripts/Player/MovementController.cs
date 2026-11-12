using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(200)]
[RequireComponent(typeof(Rigidbody), typeof(GroundSpring))]
public class MovementController : MonoBehaviour
{
    [Header("References")]
    public Transform yawPivot;
    public Transform viewPivot;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float acceleration = 80.0f;
    [Range(0.0f, 100.0f)]
    public float airAcceleratonPercentage = 15.0f;
    public float braking = 80.0f;
    public float velocityResponse = 10.0f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.08f;
    public float minPitch = -85.0f;
    public float maxPitch = 85.0f;

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
        {
            jumpQueued = true;
            jumpPressedTime = Time.fixedTime;
        }
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
            yawPivot.localRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        if (viewPivot != null)
            viewPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }


    private void FixedUpdate()
    {
        if (groundSpring.IsGrounded)
            lastGroundedTime = Time.fixedTime;
        HandleMovement();
        if (groundMotionTransfer != null)
            groundMotionTransfer.Apply();
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
        if (
            groundMotionTransfer != null &&
            groundMotionTransfer.HasSupport &&
            groundSpring.SupportNormalForce > 0.0f
        )
        {
            Vector3 desiredRelativeVelocity = direction * moveSpeed;
            if (wallContactSmoother.TryGetWallNormal(out Vector3 wallNormal1))
            {
                float intoWall = Vector3.Dot(desiredRelativeVelocity, wallNormal1);
                if (intoWall < 0.0f)
                    desiredRelativeVelocity -= wallNormal1 * intoWall;
            }
            groundMotionTransfer.SetDesiredRelativeVelocity(desiredRelativeVelocity);
            return;
        }
        if (moveInput.sqrMagnitude < 0.001f && !groundSpring.IsGrounded)
            return;

        Vector3 desiredVelocity = direction * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        if (groundSpring.IsGrounded && groundMotionTransfer != null && groundMotionTransfer.HasSupport)
            currentVelocity -= groundMotionTransfer.CarriedVelocity;

        currentVelocity.y = 0.0f;
        Vector3 velocityError = desiredVelocity - currentVelocity;
        bool hasGroundMotor = groundSpring.IsGrounded && (groundMotionTransfer == null || !groundMotionTransfer.HasSupport);
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
        groundSpring.ReleaseForJump();
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpSpeed;
        rb.linearVelocity = velocity;
        lastGroundedTime = float.NegativeInfinity;
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
