using System;
using System.Collections.Generic;
using System.Text;
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
            mode = Vector2Input.SourceMode.Tilt,
            motionSensitivity = new(10.0f, 10.0f),
            cullMotionBounds = true
        }
    };
    [SerializeField] private Vector2Input.Source[] scrollBindings =
    {
        new()
        {
            binding = "<Mouse>/scroll",
            mode = Vector2Input.SourceMode.Scroll,
            scale = 0.01f
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
        new() { binding = "<Gamepad>/rightStick", mode = Vector2Input.SourceMode.Direction }
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
            pressTime = 0.5f
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
    public Vector2Input Scroll { get; private set; }
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
        Scroll = new("Scroll", scrollBindings);
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
        Scroll.Enable();
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
        Scroll?.Dispose();
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







    // Hello. I am a firebreak. Don't mind me. I am here so things don't collapse. :)









    public InputBindingDisplayData[] GetBindingDisplayData(string inputName)
        => NormalizeInputName(inputName) switch
        {
            "pause" => CreateReleasableDisplayData(pauseBindings),
            "movecursor" => CreateVector2DisplayData(moveCursorBindings),
            "scroll" => CreateVector2DisplayData(scrollBindings),
            "menuselect" => CreateReleasableDisplayData(menuSelectBindings),
            "menuback" => CreateReleasableDisplayData(menuBackBindings),
            "menucontext" => CreateReleasableDisplayData(menuContextBindings),

            "move" => CreateVector2DisplayData(moveBindings),
            "jump" => CreatePathDisplayData(jumpBindings),
            "movecamera" => CreateVector2DisplayData(moveCamera),
            "handmode" => CreatePathDisplayData(handModeBindings),

            "resetmotioncalibration" => CreateHoldableDisplayData(resetMotionCalibration),
            "movehand" => CreateVector3DisplayData(moveHand),

            "grab" => CreatePathDisplayData(grabBindings),
            "pose1" => CreatePathDisplayData(pose1Bindings),
            "pose2" => CreatePathDisplayData(pose2Bindings),
            "pose3" => CreatePathDisplayData(pose3Bindings),
            "pose4" => CreatePathDisplayData(pose4Bindings),

            _ => Array.Empty<InputBindingDisplayData>()
        };


    private static InputBindingDisplayData[] CreateReleasableDisplayData(ReleasableButtonInput[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        foreach (ReleasableButtonInput binding in bindings)
        {
            InputBindingDisplayData data = CreateSingleBindingDisplayData(binding?.binding);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateHoldableDisplayData(HoldableButtonInput[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        foreach (HoldableButtonInput binding in bindings)
        {
            InputBindingDisplayData data = CreateSingleBindingDisplayData(binding?.binding);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreatePathDisplayData(string[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        foreach (string binding in bindings)
        {
            InputBindingDisplayData data = CreateSingleBindingDisplayData(binding);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateVector2DisplayData(Vector2Input.Source[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        foreach (Vector2Input.Source source in bindings)
        {
            if (source == null)
                continue;
            if (source.mode == Vector2Input.SourceMode.Tilt)
            {
                result.Add(new()
                {
                    ControlName = "Motion",
                    VisualType = InputBindingVisualType.Tilt,
                    IconClassName = "motion-tilt"
                });
                continue;
            }
            if (source.useComposite)
            {
                result.Add(CreateCompositeDisplayData(
                    new[]
                    {
                        source.up,
                        source.left,
                        source.down,
                        source.right
                    },
                    InputBindingVisualType.KeyCluster4
                ));
                continue;
            }
            InputBindingDisplayData data = CreateSingleBindingDisplayData(source.binding);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateVector3DisplayData(Vector3Input.Source[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        foreach (Vector3Input.Source source in bindings)
        {
            if (source == null)
                continue;
            if (source.mode == Vector3Input.SourceMode.Tilt)
            {
                result.Add(new()
                {
                    ControlName = "Motion",
                    VisualType = InputBindingVisualType.Tilt,
                    IconClassName = "motion-tilt"
                });
                continue;
            }
            if (source.useComposite)
            {
                result.Add(CreateCompositeDisplayData(
                    new[]
                    {
                        source.up,
                        source.down,
                        source.left,
                        source.right,
                        source.forward,
                        source.backward
                    },
                    InputBindingVisualType.KeyCluster6
                ));
                continue;
            }
            InputBindingDisplayData data = CreateSingleBindingDisplayData(source.binding);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }


    private static InputBindingDisplayData CreateCompositeDisplayData(string[] bindings, InputBindingVisualType visualType)
    {
        bool hasBinding = false;
        foreach (string binding in bindings)
            if (TryParseBindingPath(binding, out _, out _))
            {
                hasBinding = true;
                break;
            }
        if (!hasBinding)
            return null;
        string[] labels = new string[bindings.Length];
        for (int i = 0; i < bindings.Length; i++)
            labels[i] = GetBindingLabel(bindings[i]);
        return new()
        {
            ControlName = GetCompositeControlName(bindings),
            VisualType = visualType,
            KeyLabels = labels
        };
    }
    private static InputBindingDisplayData CreateSingleBindingDisplayData(string binding)
    {
        if (!TryParseBindingPath(binding, out string device, out string control))
            return null;
        string controlName = GetDeviceDisplayName(device);
        if (device.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return new()
            {
                ControlName = controlName,
                VisualType = InputBindingVisualType.Key,
                KeyLabels = new[] { FormatKeyboardKey(control) }
            };
        if (device.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
        {
            if (control.Equals("delta", StringComparison.OrdinalIgnoreCase))
                return new()
                {
                    ControlName = controlName,
                    VisualType = InputBindingVisualType.MouseDelta,
                    IconClassName = "mouse-delta"
                };
            if (control.Equals("scroll", StringComparison.OrdinalIgnoreCase))
                return new()
                {
                    ControlName = controlName,
                    VisualType = InputBindingVisualType.ScrollWheel,
                    IconClassName = "mouse-scroll"
                };
            if (control.EndsWith("Button", StringComparison.OrdinalIgnoreCase))
                return new()
                {
                    ControlName = controlName,
                    VisualType = InputBindingVisualType.MouseButton,
                    IconClassName = CreateIconClassName(device, control)
                };
        }
        return new()
        {
            ControlName = controlName,
            VisualType = InputBindingVisualType.GenericIcon,
            IconClassName = CreateIconClassName(device, control)
        };
    }


    private static bool TryParseBindingPath(string binding, out string device, out string control)
    {
        device = null;
        control = null;
        if (string.IsNullOrWhiteSpace(binding))
            return false;
        int deviceStart = binding.IndexOf('<');
        int deviceEnd = binding.IndexOf('>');
        if (deviceStart < 0 || deviceEnd < 0)
            return false;
        int controlStart = binding.IndexOf('/', deviceEnd + 1);
        if (controlStart < 0 || controlStart >= binding.Length - 1)
            return false;
        device = binding.Substring(deviceStart + 1, deviceEnd - deviceStart - 1);
        control = binding.Substring(controlStart + 1);
        return !string.IsNullOrWhiteSpace(device) && !string.IsNullOrWhiteSpace(control);
    }
    private static string GetBindingLabel(string binding)
    {
        if (!TryParseBindingPath(binding, out string device, out string control))
            return string.Empty;
        if (device.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return FormatKeyboardKey(control);
        return FormatDisplayWords(control).ToUpperInvariant();
    }
    private static string GetCompositeControlName(string[] bindings)
    {
        string commonDevice = null;
        foreach (string binding in bindings)
        {
            if (!TryParseBindingPath(binding, out string device, out _))
                continue;
            if (commonDevice == null)
            {
                commonDevice = device;
                continue;
            }
            if (!commonDevice.Equals(device, StringComparison.OrdinalIgnoreCase))
                return "Mixed Controls";
        }
        return commonDevice != null
            ? GetDeviceDisplayName(commonDevice) : "Controls";
    }
    private static string GetDeviceDisplayName(string device)
    {
        if (device.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return "Keyboard";
        if (device.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
            return "Mouse";
        if (device.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
            return "Gamepad";
        return FormatDisplayWords(device);
    }
    private static string NormalizeInputName(string inputName)
    {
        if (string.IsNullOrWhiteSpace(inputName))
            return string.Empty;
        StringBuilder result = new(inputName.Length);
        foreach (char character in inputName)
            if (char.IsLetterOrDigit(character))
                result.Append(char.ToLowerInvariant(character));
        return result.ToString();
    }


    private static string FormatKeyboardKey(string control)
        => control.ToLowerInvariant() switch
        {
            "space" => "SPACE",
            "enter" => "ENTER",
            "escape" => "ESC",
            "tab" => "TAB",
            "backspace" => "BACK",
            "delete" => "DEL",

            "leftctrl" or "leftcontrol" => "L CTRL",
            "rightctrl" or "rightcontrol" => "R CTRL",
            "leftshift" => "L SHIFT",
            "rightshift" => "R SHIFT",
            "leftalt" => "L ALT",
            "rightalt" => "R ALT",
            "leftmeta" => "L META",
            "rightmeta" => "R META",

            "uparrow" => "↑",
            "downarrow" => "↓",
            "leftarrow" => "←",
            "rightarrow" => "→",

            "pageup" => "PG UP",
            "pagedown" => "PG DN",
            "home" => "HOME",
            "end" => "END",

            _ => FormatDisplayWords(control).ToUpperInvariant()
        };
    private static string FormatDisplayWords(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        StringBuilder result = new();
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            if (character is '/' or '_' or '-')
            {
                if (result.Length > 0 && result[result.Length - 1] != ' ')
                    result.Append(' ');
                continue;
            }
            if (char.IsUpper(character) && i > 0 && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1])))
                result.Append(' ');
            result.Append(character);
        }
        return result.ToString().Trim();
    }
    private static string CreateIconClassName(string device, string control)
        => $"{ToKebabCase(device)}-{ToKebabCase(control)}";
    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        StringBuilder result = new();
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            if (character is '/' or '_' or '-' or ' ')
            {
                if (result.Length > 0 && result[result.Length - 1] != '-')
                    result.Append('-');
                continue;
            }
            if (char.IsUpper(character) && i > 0 && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1])))
                result.Append('-');
            result.Append(char.ToLowerInvariant(character));
        }
        return result.ToString().Trim('-');
    }
}
