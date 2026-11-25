using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "Scriptable Objects/Player Input")]
public class PlayerInput : ScriptableObject
{
    [Header("UI Bindings")]
    [SerializeField] private ReleasableButtonInput[] pauseBindings =
    {
        new() { binding = "<Keyboard>/escape" },
        new() { binding = "<Gamepad>/start" }
    };
    [SerializeField] private Vector2Input.Source[] moveCursorBindings =
    {
        new() { binding = "<Mouse>/delta", mode = Vector2Input.SourceMode.Relative },
        new()
        {
            useComposite = true,
            mode = Vector2Input.SourceMode.Direction,
            up = "<Keyboard>/w",
            down = "<Keyboard>/s",
            left = "<Keyboard>/a",
            right = "<Keyboard>/d"
        },
        new()
        {
            useComposite = true,
            mode = Vector2Input.SourceMode.Direction,
            up = "<Keyboard>/upArrow",
            down = "<Keyboard>/downArrow",
            left = "<Keyboard>/leftArrow",
            right = "<Keyboard>/rightArrow"
        },
        new()
        {
            mode = Vector2Input.SourceMode.Tilt,
            motionSensitivity = new(10.0f, 10.0f),
            cullMotionBounds = true
        }
    };
    [SerializeField] private ReleasableButtonInput[] menuSelectBindings =
    {
        new() { binding = "<Keyboard>/e" },
        new() { binding = "<Keyboard>/enter" },
        new() { binding = "<Keyboard>/space" },
        new() { binding = "<Mouse>/leftButton" },
        new() { binding = "<Gamepad>/buttonSouth" }
    };
    [SerializeField] private ReleasableButtonInput[] menuBackBindings =
    {
        new() { binding = "<Keyboard>/tab" },
        new() { binding = "<Keyboard>/backspace" },
        new() { binding = "<Keyboard>/leftAlt" },
        new() { binding = "<Mouse>/backButton" },
        new() { binding = "<Gamepad>/buttonEast" }
    };
    [SerializeField] private ReleasableButtonInput[] menuContextBindings =
    {
        new() { binding = "<Keyboard>/q" },
        new() { binding = "<Keyboard>/rightShift" },
        new() { binding = "<Keyboard>/leftControl" },
        new() { binding = "<Mouse>/rightButton" },
        new() { binding = "<Gamepad>/buttonWest" }
    };

    [Header("General Bindings")]
    [SerializeField] private Vector2Input.Source[] moveBindings =
    {
        new() { binding = "<Gamepad>/leftStick", mode = Vector2Input.SourceMode.Direction },
        new()
        {
            useComposite = true,
            mode = Vector2Input.SourceMode.Direction,
            up = "<Keyboard>/w",
            down = "<Keyboard>/s",
            left = "<Keyboard>/a",
            right = "<Keyboard>/d"
        }
    };
    [SerializeField] private string[] jumpBindings =
    {
        "<Keyboard>/space",
        "<Mouse>/middleButton",
        "<Gamepad>/buttonSouth"
    };
    [SerializeField] private Vector2Input.Source[] moveCamera =
    {
        new() { binding = "<Mouse>/delta", mode = Vector2Input.SourceMode.Relative },
        new() { binding = "<Gamepad>/rightStick", mode = Vector2Input.SourceMode.Direction },
        new()
        {
            useComposite = true,
            mode = Vector2Input.SourceMode.Direction,
            up = "<Keyboard>/upArrow",
            down = "<Keyboard>/downArrow",
            left = "<Keyboard>/leftArrow",
            right = "<Keyboard>/rightArrow"
        }
    };
    [SerializeField] private string[] handModeBindings =
    {
        "<Keyboard>/leftAlt",
        "<Mouse>/rightButton",
        "<Gamepad>/leftShoulder"
    };

    [Header("Motion controls")]
    [SerializeField] private HoldableButtonInput[] resetMotionCalibration =
    {
        new()
        {
            binding = "<Gamepad>/start",
            pressTime = 1.0f
        }
    };
    [SerializeField] private Vector3Input.Source[] moveHand =
    {
        new() { binding = "<Mouse>/delta", mode = Vector3Input.SourceMode.Relative },
        new()
        {
            useComposite = true,
            mode = Vector3Input.SourceMode.Direction,
            up = "<Keyboard>/upArrow",
            down = "<Keyboard>/downArrow",
            left = "<Keyboard>/leftArrow",
            right = "<Keyboard>/rightArrow"
        },
        new() {
            mode = Vector3Input.SourceMode.Tilt,
            motionSensitivity = new(10.0f, 10.0f, 1.0f),
            cullMotionBounds = true
        }
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

    public ReleasableButtonInput Pause { get; private set;}
    public Vector2Input MoveCursor { get; private set; }
    public ReleasableButtonInput MenuSelect { get; private set; }
    public ReleasableButtonInput MenuBack { get; private set; }
    public ReleasableButtonInput MenuContext { get; private set; }
    public Vector2Input Move { get; private set; }
    public InputAction Jump { get; private set; }
    public Vector2Input MoveCamera { get; private set; }
    public InputAction HandMode { get; private set; }
    public HoldableButtonInput ResetMotionCalibration { get; private set; }
    public Vector3Input MoveHand { get; private set; }
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
        Pause = new("Pause", pauseBindings);
        MoveCursor = new("Move Cursor", moveCursorBindings);
        MenuSelect = new("Menu Select", menuSelectBindings);
        MenuBack = new("Menu Back", menuBackBindings);
        MenuContext = new("Menu Context", menuContextBindings);

        Move = new("Move", moveBindings);
        Jump = CreateButton("Jump", jumpBindings);
        MoveCamera = new("Move Camera", moveCamera);
        HandMode = CreateButton("Hand Mode", handModeBindings);
        
        MoveHand = new("Move Hand", moveHand);
        ResetMotionCalibration = new("Reset Motion Calibration", resetMotionCalibration);

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
        Pause.Enable();
        MoveCursor.Enable();
        MenuSelect.Enable();
        MenuBack.Enable();
        MenuContext.Enable();

        Move.Enable();
        Jump.Enable();
        MoveCamera.Enable();
        HandMode.Enable();

        MoveHand.Enable();
        ResetMotionCalibration.Enable();

        Grab.Enable();
        Pose1.Enable();
        Pose2.Enable();
        Pose3.Enable();
        Pose4.Enable();
    }

    private void DisableAndDisposeActions()
    {
        Pause?.Dispose();
        MoveCursor?.Dispose();
        MenuSelect?.Dispose();
        MenuBack?.Dispose();
        MenuContext?.Dispose();

        Move?.Dispose();
        DisposeAction(Jump);
        MoveCamera?.Dispose();
        DisposeAction(HandMode);

        MoveHand?.Dispose();
        ResetMotionCalibration?.Dispose();

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
