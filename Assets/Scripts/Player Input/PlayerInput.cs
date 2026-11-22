using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "Scriptable Objects/Player Input")]
public class PlayerInput : ScriptableObject
{
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
        "<Keyboard>/leftShift",
        "<Mouse>/rightButton",
        "<Gamepad>/leftShoulder"
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
        new()
        {
            mode = Vector3Input.SourceMode.Tilt,
            tiltSensitivity = 8.0f
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

    public Vector2Input Move { get; private set; }
    public InputAction Jump { get; private set; }
    public Vector2Input MoveCamera { get; private set; }
    public InputAction HandMode { get; private set; }
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
        Move = new("Move", moveBindings);
        Jump = CreateButton("Jump", jumpBindings);
        MoveCamera = new("Move Camera", moveCamera);

        HandMode = CreateButton("Hand Mode", handModeBindings);
        MoveHand = new("Move Hand", moveHand);
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
        MoveCamera.Enable();
        HandMode.Enable();
        MoveHand.Enable();
        Grab.Enable();
        Pose1.Enable();
        Pose2.Enable();
        Pose3.Enable();
        Pose4.Enable();
    }

    private void DisableAndDisposeActions()
    {
        Move?.Dispose();
        DisposeAction(Jump);
        MoveCamera?.Dispose();
        DisposeAction(HandMode);
        MoveHand?.Dispose();
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
