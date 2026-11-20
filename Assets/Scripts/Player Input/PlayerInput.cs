using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "Scriptable Objects/Player Input")]
public class PlayerInput : ScriptableObject
{
    [Header("Movement Bindings")]
    [SerializeField] private string moveForward = "<Keyboard>/w";
    [SerializeField] private string moveBack = "<Keyboard>/s";
    [SerializeField] private string moveLeft = "<Keyboard>/a";
    [SerializeField] private string moveRight = "<Keyboard>/d";
    [SerializeField] private string moveStick = "<Gamepad>/leftStick";

    [Header("General Bindings")]
    [SerializeField] private string[] jumpBindings =
    {
        "<Keyboard>/space",
        "<Mouse>/middleButton",
        "<Gamepad>/buttonSouth"
    };
    [SerializeField] private string[] unlockCursorBindings =
    {
        "<Keyboard>/escape",
        "<Mouse>/backButton",
        "<Gamepad>/start"
    };
    [SerializeField] private string[] lockCursorBindings =
    {
        "<Mouse>/leftButton",
        "<Gamepad>/buttonSouth"
    };
    [SerializeField] private string[] handModeBindings =
    {
        "<Keyboard>/leftShift",
        "<Mouse>/rightButton",
        "<Gamepad>/leftShoulder"
    };

    [Header("Hand pose bindings")]
    [SerializeField] private string[] grabBindings =
    {
        "<Mouse>/leftButton",
        "<Keyboard>/leftCtrl",
        "<Gamepad>/rightTrigger"
    };
    [SerializeField] private string[] pose1Bindings =
    {
        "<Keyboard>/1",
        "<Gamepad>/dpad/up"
    };
    [SerializeField] private string[] pose2Bindings =
    {
        "<Keyboard>/2",
        "<Gamepad>/dpad/right"
    };
    [SerializeField] private string[] pose3Bindings =
    {
        "<Keyboard>/3",
        "<Gamepad>/dpad/down"
    };
    [SerializeField] private string[] pose4Bindings =
    {
        "<Keyboard>/4",
        "<Gamepad>/dpad/left"
    };

    public InputAction Move { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction UnlockCursor { get; private set; }
    public InputAction LockCursor { get; private set; }
    public InputAction HandMode { get; private set; }
    public InputAction Grab { get; private set; }
    public InputAction Pose1 { get; private set; }
    public InputAction Pose2 { get; private set; }
    public InputAction Pose3 { get; private set; }
    public InputAction Pose4 { get; private set; }


    private void OnEnable()
    {
        BuildActions();
        EnableActions();
    }

    private void OnDisable()
        => DisableAndDisposeActions();


    private void BuildActions()
    {
        Move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        Move.AddCompositeBinding("2DVector")
            .With("Up", moveForward)
            .With("Down", moveBack)
            .With("Left", moveLeft)
            .With("Right", moveRight);
        Move.AddBinding(moveStick);

        Jump = CreateButton("Jump", jumpBindings);
        UnlockCursor = CreateButton("Unlock Cursor", unlockCursorBindings);
        LockCursor = CreateButton("Lock Cursor", lockCursorBindings);
        HandMode = CreateButton("Hand Mode", handModeBindings);
        Grab = CreateButton("Grab", grabBindings);
        Pose1 = CreateButton("Pose 1", pose1Bindings);
        Pose2 = CreateButton("Pose 2", pose2Bindings);
        Pose3 = CreateButton("Pose 3", pose3Bindings);
        Pose4 = CreateButton("Pose 4", pose4Bindings);
    }

    private static InputAction CreateButton(string name, string[] paths)
    {
        InputAction action = new(name, InputActionType.Button);
        if (paths != null)
            foreach (string path in paths)
                if (!string.IsNullOrWhiteSpace(path))
                    action.AddBinding(path);
        return action;
    }

    private void EnableActions()
    {
        Move.Enable();
        Jump.Enable();
        UnlockCursor.Enable();
        LockCursor.Enable();
        HandMode.Enable();
        Grab.Enable();
        Pose1.Enable();
        Pose2.Enable();
        Pose3.Enable();
        Pose4.Enable();
    }

    private void DisableAndDisposeActions()
    {
        DisposeAction(Move);
        DisposeAction(Jump);
        DisposeAction(UnlockCursor);
        DisposeAction(LockCursor);
        DisposeAction(HandMode);
        DisposeAction(Grab);
        DisposeAction(Pose1);
        DisposeAction(Pose2);
        DisposeAction(Pose3);
        DisposeAction(Pose4);
    }

    private static void DisposeAction(InputAction action)
    {
        if (action == null)
            return;
        action.Disable();
        action.Dispose();
    }
}
