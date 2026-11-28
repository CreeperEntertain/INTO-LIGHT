using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "Scriptable Objects/Player Input")]
public class PlayerInput : ScriptableObject
{
    [Header("Icons")]
    [SerializeField] private InputIconLibrary inputIconLibrary;

    [Header("SHOULD NEVER CHANGE!")]
    [SerializeField] private HoldableButtonInput[] resetKeybinds =
    {
        new() { binding = "<Keyboard>/r", pressTime = 2.0f },
        new() { binding = "<Gamepad>/select", pressTime = 2.0f }
    };

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

    public HoldableButtonInput ResetKeybinds { get; private set; }
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


    public InputDevice ActiveDevice { get; private set; }
    public string ActiveDeviceName { get; private set; }

    public event Action ActiveDeviceChanged;


    private void OnEnable()
    {
        ActiveDevice = null;
        ActiveDeviceName = null;
        InputSystem.onEvent += OnInputEvent;
        BuildActions();
        EnableActions();
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        DisableAndDisposeActions();
    }


    private void BuildActions()
    {
        ResetKeybinds = new("Reset Keybinds", resetKeybinds);

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
        ResetKeybinds.Enable();

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
        ResetKeybinds?.Dispose();

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

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null || device is Sensor)
            return;
        if (eventPtr.type != StateEvent.Type && eventPtr.type != DeltaStateEvent.Type)
            return;
        foreach (InputControl control in eventPtr.EnumerateChangedControls(device: device, magnitudeThreshold: 0.15f))
        {
            SetActiveDevice(device);
            return;
        }
    }

    private void SetActiveDevice(InputDevice device)
    {
        if (ActiveDevice == device)
            return;
        ActiveDevice = device;
        ActiveDeviceName = GetDeviceDisplayName(device);
        ActiveDeviceChanged?.Invoke();
    }

    public HoldableButtonInput GetHoldableButtonInput(string inputName)
        => NormalizeInputName(inputName) switch
        {
            "resetkeybinds" => ResetKeybinds,
            "resetmotioncalibration" => ResetMotionCalibration,
            _ => null
        };
    
    public void RebuildActions()
    {
        DisableAndDisposeActions();
        BuildActions();
        EnableActions();
    }







    // Hello. I am a firebreak. Don't mind me. I am here so things don't collapse. :)









    public InputBindingDisplayData[] GetBindingDisplayData(string inputName)
    {
        string normalized = NormalizeInputName(inputName);

        InputBindingDisplayData[] result = normalized switch
        {
            "resetkeybinds" => CreateHoldableDisplayData(normalized, resetKeybinds),
            "pause" => CreateReleasableDisplayData(normalized, pauseBindings),
            "movecursor" => CreateVector2DisplayData(normalized, moveCursorBindings),
            "scroll" => CreateVector2DisplayData(normalized, scrollBindings),
            "menuselect" => CreateReleasableDisplayData(normalized, menuSelectBindings),
            "menuback" => CreateReleasableDisplayData(normalized, menuBackBindings),
            "menucontext" => CreateReleasableDisplayData(normalized, menuContextBindings),
            "move" => CreateVector2DisplayData(normalized, moveBindings),
            "jump" => CreatePathDisplayData(normalized, jumpBindings),
            "movecamera" => CreateVector2DisplayData(normalized, moveCamera),
            "handmode" => CreatePathDisplayData(normalized, handModeBindings),
            "resetmotioncalibration" => CreateHoldableDisplayData(normalized, resetMotionCalibration),
            "movehand" => CreateVector3DisplayData(normalized, moveHand),
            "grab" => CreatePathDisplayData(normalized, grabBindings),
            "pose1" => CreatePathDisplayData(normalized, pose1Bindings),
            "pose2" => CreatePathDisplayData(normalized, pose2Bindings),
            "pose3" => CreatePathDisplayData(normalized, pose3Bindings),
            "pose4" => CreatePathDisplayData(normalized, pose4Bindings),
            _ => Array.Empty<InputBindingDisplayData>()
        };
        foreach (InputBindingDisplayData data in result)
            if (data != null)
                ResolveDisplayIcons(data);
        return result;
    }
    private void ResolveDisplayIcons(InputBindingDisplayData data)
    {
        data.EmptyIcon = inputIconLibrary != null
            ? inputIconLibrary.ResolveIcon("empty", out _) : null;
        if (data.VisualType is InputBindingVisualType.IconCluster4 or InputBindingVisualType.IconCluster6)
        {
            if (data.IconNames == null)
                data.IconNames = Array.Empty<string>();
            data.Icons = new UnityEngine.Object[data.IconNames.Length];
            for (int i = 0; i < data.IconNames.Length; i++)
            {
                string resolvedName = data.IconNames[i];
                if (inputIconLibrary != null)
                    data.Icons[i] = inputIconLibrary.ResolveIcon(data.IconNames[i], out resolvedName);
                data.IconNames[i] = resolvedName;
            }
            return;
        }

        string iconName = data.IconName;
        if (inputIconLibrary != null)
            data.Icon = inputIconLibrary.ResolveIcon(data.IconName, out iconName);
        data.IconName = iconName;
    }


    private static InputBindingDisplayData[] CreateReleasableDisplayData(string inputName, ReleasableButtonInput[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        for (int i = 0; i < bindings.Length; i++)
        {
            ReleasableButtonInput binding = bindings[i];
            if (binding == null)
                continue;
            InputBindingDisplayData data = CreateSingleBindingDisplayData(binding.binding, inputName, i, "binding", InputBindingValueType.Button, path => binding.binding = path);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateHoldableDisplayData(string inputName, HoldableButtonInput[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        for (int i = 0; i < bindings.Length; i++)
        {
            HoldableButtonInput binding = bindings[i];
            if (binding == null)
                continue;
            InputBindingDisplayData data = CreateSingleBindingDisplayData(binding.binding, inputName, i, "binding", InputBindingValueType.Button, path => binding.binding = path);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreatePathDisplayData(string inputName, string[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        for (int i = 0; i < bindings.Length; i++)
        {
            int sourceIndex = i;
            InputBindingDisplayData data = CreateSingleBindingDisplayData(bindings[i], inputName, sourceIndex, "binding", InputBindingValueType.Button, path => bindings[sourceIndex] = path);
            if (data != null)
                result.Add(data);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateVector2DisplayData(string inputName, Vector2Input.Source[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        for (int i = 0; i < bindings.Length; i++)
        {
            Vector2Input.Source source = bindings[i];
            if (source == null)
                continue;
            if (source.mode == Vector2Input.SourceMode.Tilt)
            {
                result.Add(new()
                {
                    ControlName = "Motion",
                    VisualType = InputBindingVisualType.Icon,
                    IconName = "motion-tilt"
                });
                continue;
            }
            if (source.useComposite)
            {
                InputBindingDisplayData data = CreateCompositeDisplayData(
                    inputName,
                    i,
                    new[]
                    {
                        source.up,
                        source.left,
                        source.down,
                        source.right
                    },
                    new[] { "up", "left", "down", "right" },
                    InputBindingVisualType.IconCluster4,
                    (component, path) =>
                    {
                        switch (component)
                        {
                            case "up":
                                source.up = path;
                                break;
                            case "left":
                                source.left = path;
                                break;
                            case "down":
                                source.down = path;
                                break;
                            case "right":
                                source.right = path;
                                break;
                        }
                    });
                if (data != null)
                    result.Add(data);
                continue;
            }
            InputBindingDisplayData single = CreateSingleBindingDisplayData(source.binding, inputName, i, "binding", InputBindingValueType.Vector2, path => source.binding = path);
            if (single != null)
                result.Add(single);
        }
        return result.ToArray();
    }
    private static InputBindingDisplayData[] CreateVector3DisplayData(string inputName, Vector3Input.Source[] bindings)
    {
        List<InputBindingDisplayData> result = new();
        if (bindings == null)
            return result.ToArray();
        for (int i = 0; i < bindings.Length; i++)
        {
            Vector3Input.Source source = bindings[i];
            if (source == null)
                continue;
            if (source.mode == Vector3Input.SourceMode.Tilt)
            {
                result.Add(new()
                {
                    ControlName = "Motion",
                    VisualType = InputBindingVisualType.Icon,
                    IconName = "motion-tilt"
                });
                continue;
            }
            if (source.useComposite)
            {
                InputBindingDisplayData data = CreateCompositeDisplayData(
                    inputName,
                    i,
                    new[]
                    {
                        source.up,
                        source.down,
                        source.left,
                        source.right,
                        source.forward,
                        source.backward
                    },
                    new[] { "up", "down", "left", "right", "forward", "backward" },
                    InputBindingVisualType.IconCluster6,
                    (component, path) =>
                    {
                        switch (component)
                        {
                            case "up":
                                source.up = path;
                                break;
                            case "down":
                                source.down = path;
                                break;
                            case "left":
                                source.left = path;
                                break;
                            case "right":
                                source.right = path;
                                break;
                            case "forward":
                                source.forward = path;
                                break;
                            case "backward":
                                source.backward = path;
                                break;
                        }
                    });
                if (data != null)
                    result.Add(data);
                continue;
            }
            InputBindingDisplayData single = CreateSingleBindingDisplayData(source.binding, inputName, i, "binding", InputBindingValueType.Vector2, path => source.binding = path);
            if (single != null)
                result.Add(single);
        }
        return result.ToArray();
    }


    private static InputBindingDisplayData CreateCompositeDisplayData(
        string inputName,
        int sourceIndex,
        string[] bindings,
        string[] componentNames,
        InputBindingVisualType visualType,
        Action<string, string> applyComponent
    )
    {
        bool hasBinding = false;
        foreach (string binding in bindings)
        {
            if (!TryParseBindingPath(binding, out _, out _))
                continue;
            hasBinding = true;
            break;
        }
        if (!hasBinding)
            return null;
        string[] iconNames = new string[bindings.Length];
        List<InputBindingTarget> targets = new();
        for (int i = 0; i < bindings.Length; i++)
        {
            iconNames[i] = GetBindingIconName(bindings[i]);
            if (!TryParseBindingPath(bindings[i], out _, out _))
                continue;
            string componentName = componentNames[i];
            targets.Add(new InputBindingTarget
            {
                InputName = inputName,
                SourceIndex = sourceIndex,
                ComponentName = componentName,
                CurrentPath = bindings[i],
                ValueType = InputBindingValueType.Button,
                ApplyPath = path => applyComponent(componentName, path)
            });
        }
        return new InputBindingDisplayData
        {
            ControlName = GetCompositeControlName(bindings),
            VisualType = visualType,
            IconNames = iconNames,
            BindingTargets = targets.ToArray()
        };
    }
    private static InputBindingDisplayData CreateSingleBindingDisplayData(
        string binding,
        string inputName,
        int sourceIndex,
        string componentName,
        InputBindingValueType valueType,
        Action<string> applyPath
    )
    {
        if (!TryParseBindingPath(binding, out string device, out string control))
            return null;
        InputBindingTarget target = new()
        {
            InputName = inputName,
            SourceIndex = sourceIndex,
            ComponentName = componentName,
            CurrentPath = binding,
            ValueType = valueType,
            ApplyPath = applyPath
        };
        return new InputBindingDisplayData
        {
            ControlName = GetDeviceDisplayName(device),
            VisualType = InputBindingVisualType.Icon,
            IconName = GetBindingIconName(device, control),
            BindingTargets = new[] { target }
        };
    }


    private static string GetBindingIconName(string binding)
    {
        if (!TryParseBindingPath(binding, out string device, out string control))
            return "empty";
        return GetBindingIconName(device, control);
    }
    private static string GetBindingIconName(string device, string control)
    {
        if (device.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
        {
            if (control.Equals("delta", StringComparison.OrdinalIgnoreCase))
                return "mouse-delta";
            if (control.Equals("scroll", StringComparison.OrdinalIgnoreCase))
                return "mouse-scroll";
        }
        return CreateIconName(device, control);
    }
    private static string CreateIconName(string device, string control)
        => $"{ToKebabCase(device)}-{ToKebabCase(control)}";


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
    private static string GetDeviceDisplayName(InputDevice device)
    {
        if (device is Keyboard)
            return "Keyboard";
        if (device is Mouse)
            return "Mouse";
        if (device is Gamepad)
            return "Gamepad";
        return GetDeviceDisplayName(device.layout);
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



    public InputBindingOption[] GetAvailableBindingOptions(
        InputBindingTarget target)
    {
        if (target == null || string.IsNullOrWhiteSpace(target.CurrentPath))
            return Array.Empty<InputBindingOption>();
        if (!TryParseBindingPath(target.CurrentPath, out string deviceLayout, out _))
            return Array.Empty<InputBindingOption>();
        List<string> paths = new();
        AddBindingPath(paths, target.CurrentPath);
        AddFallbackBindingPaths(deviceLayout, target.ValueType, paths);
        foreach (InputDevice device in InputSystem.devices)
        {
            if (!IsCompatibleDevice(device, deviceLayout))
                continue;
            foreach (InputControl control in device.allControls)
            {
                if (!IsCompatibleControl(control, target.ValueType))
                    continue;
                string controlPath = control.path;
                string devicePath = device.path;
                if (!controlPath.StartsWith(devicePath, StringComparison.OrdinalIgnoreCase))
                    continue;
                controlPath = controlPath
                    .Substring(devicePath.Length)
                    .Trim('/');
                if (string.IsNullOrWhiteSpace(controlPath))
                    continue;
                AddBindingPath(paths, $"<{deviceLayout}>/{controlPath}");
            }
        }
        List<InputBindingOption> result = new();
        HashSet<string> usedLabels = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            string label = GetBindingLabel(path);
            if (string.IsNullOrWhiteSpace(label))
                label = path;
            if (!usedLabels.Add(label))
            {
                string suffix = path.Substring(path.LastIndexOf('/') + 1);
                string baseLabel = $"{label} ({suffix})";
                label = baseLabel;
                int number = 2;
                while (!usedLabels.Add(label))
                {
                    label = $"{baseLabel} {number}";
                    number++;
                }
            }
            result.Add(new InputBindingOption
            {
                Label = label,
                Path = path
            });
        }
        return result.ToArray();
    }

    private static bool IsCompatibleDevice(InputDevice device, string deviceLayout)
    {
        if (device == null)
            return false;
        if (deviceLayout.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return device is Keyboard;
        if (deviceLayout.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
            return device is Mouse;
        if (deviceLayout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
            return device is Gamepad;
        return device.layout.Equals(deviceLayout, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCompatibleControl(InputControl control, InputBindingValueType valueType)
    {
        if (control == null)
            return false;
        return valueType switch
        {
            InputBindingValueType.Button => control is ButtonControl,
            InputBindingValueType.Vector2 => control is Vector2Control,
            InputBindingValueType.Vector3 => control is Vector3Control,
            _ => false,
        };
    }

    private static void AddBindingPath(List<string> paths, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;
        foreach (string existing in paths)
            if (existing.Equals(path, StringComparison.OrdinalIgnoreCase))
                return;
        paths.Add(path);
    }

    private static void AddFallbackBindingPaths(string deviceLayout, InputBindingValueType valueType, List<string> paths)
    {
        if (deviceLayout.Equals("Keyboard", StringComparison.OrdinalIgnoreCase) && valueType == InputBindingValueType.Button)
        {
            for (char key = 'a'; key <= 'z'; key++)
                AddBindingPath(paths, $"<Keyboard>/{key}");
            for (char key = '0'; key <= '9'; key++)
                AddBindingPath(paths, $"<Keyboard>/{key}");
            string[] controls =
            {
                "space", "enter", "escape", "tab",
                "backspace", "delete", "insert",
                "home", "end", "pageUp", "pageDown",
                "upArrow", "downArrow", "leftArrow", "rightArrow",
                "leftControl", "rightControl",
                "leftShift", "rightShift",
                "leftAlt", "rightAlt",
                "leftMeta", "rightMeta",
                "backquote", "quote", "semicolon", "comma",
                "period", "slash", "backslash",
                "leftBracket", "rightBracket", "minus", "equals",
                "capsLock",
                "f1", "f2", "f3", "f4", "f5", "f6",
                "f7", "f8", "f9", "f10", "f11", "f12"
            };
            foreach (string control in controls)
                AddBindingPath(paths, $"<Keyboard>/{control}");
            return;
        }

        if (deviceLayout.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
        {
            string[] controls = valueType switch
            {
                InputBindingValueType.Button => new[]
                {
                    "leftButton", "rightButton", "middleButton",
                    "forwardButton", "backButton"
                },
                InputBindingValueType.Vector2 => new[]
                {
                    "delta", "position", "scroll"
                },
                _ => Array.Empty<string>()
            };
            foreach (string control in controls)
                AddBindingPath(paths, $"<Mouse>/{control}");
            return;
        }

        if (deviceLayout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
        {
            string[] controls = valueType switch
            {
                InputBindingValueType.Button => new[]
                {
                    "buttonSouth", "buttonNorth",
                    "buttonEast", "buttonWest",
                    "start", "select",
                    "leftShoulder", "rightShoulder",
                    "leftTrigger", "rightTrigger",
                    "leftStickPress", "rightStickPress",
                    "dpad/up", "dpad/down",
                    "dpad/left", "dpad/right"
                },
                InputBindingValueType.Vector2 => new[]
                {
                    "leftStick", "rightStick", "dpad"
                },
                _ => Array.Empty<string>()
            };
            foreach (string control in controls)
                AddBindingPath(paths, $"<Gamepad>/{control}");
        }
    }
}
