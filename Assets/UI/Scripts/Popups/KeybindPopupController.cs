
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public static class KeybindPopupController
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly string[] ButtonDevices =
    {
        "Keyboard", "Mouse", "Gamepad"
    };

    private static readonly string[] Vector2Devices =
    {
        "Keyboard", "Mouse", "Gamepad", "Scroll", "Motion"
    };

    private static readonly string[] Vector3Devices =
    {
        "Keyboard", "Mouse", "Gamepad", "Motion"
    };

    public static void Show(
        PlayerInput keybinds,
        string inputName,
        int bindingIndex,
        string title,
        VisualTreeAsset contentAsset,
        VisualTreeAsset rowAsset,
        Action onApplied)
    {
        if (keybinds == null)
            return;
        if (contentAsset == null || rowAsset == null)
        {
            Debug.LogError("Keybind popup UXML assets are not assigned.");
            return;
        }

        Array bindings = GetBindings(keybinds, inputName);
        if (bindings == null || bindingIndex < 0 || bindingIndex >= bindings.Length)
        {
            Debug.LogError($"Cannot find binding {bindingIndex} for '{inputName}'.");
            return;
        }

        object entry = bindings.GetValue(bindingIndex);
        if (entry == null)
            return;

        string[] options = GetOptions(inputName, entry);
        if (options.Length == 0)
            return;

        TemplateContainer content = contentAsset.CloneTree();
        VisualElement rows = content.Q<VisualElement>("BindingRows");
        if (rows == null)
        {
            Debug.LogError("Keybind popup content is missing BindingRows.");
            return;
        }

        TemplateContainer row = rowAsset.CloneTree();
        DropdownSelector selector = row.Q<DropdownSelector>("BindingSelector");
        Button recordButton = row.Q<Button>("RecordButton");
        if (selector == null || recordButton == null)
        {
            Debug.LogError("Keybind popup row needs BindingSelector and RecordButton.");
            return;
        }

        selector.SetOptions(options, GetCurrentType(entry));
        rows.Add(row);
        string popupId = $"keybind-{Normalize(inputName)}-{bindingIndex}";
        PopupManager.ShowComplex(
            popupId, title, content,
            new PopupButton("Confirm", () =>
            {
                string selectedType = selector.value;
                if (!string.IsNullOrWhiteSpace(selectedType) && ApplyDeviceType(keybinds, inputName, bindingIndex, selectedType))
                {
                    keybinds.RebuildActions();
                    onApplied?.Invoke();
                }
            }),
            new PopupButton("Cancel", null)
        );
    }

    private static Array GetBindings(PlayerInput keybinds, string inputName)
    {
        string fieldName = Normalize(inputName) switch
        {
            "resetkeybinds" => "resetKeybinds",
            "pause" => "pauseBindings",
            "movecursor" => "moveCursorBindings",
            "scroll" => "scrollBindings",
            "menuselect" => "menuSelectBindings",
            "menuback" => "menuBackBindings",
            "menucontext" => "menuContextBindings",
            "move" => "moveBindings",
            "jump" => "jumpBindings",
            "movecamera" => "moveCamera",
            "handmode" => "handModeBindings",
            "resetmotioncalibration" => "resetMotionCalibration",
            "movehand" => "moveHand",
            "grab" => "grabBindings",
            "pose1" => "pose1Bindings",
            "pose2" => "pose2Bindings",
            "pose3" => "pose3Bindings",
            "pose4" => "pose4Bindings",
            _ => null
        };
        if (fieldName == null)
            return null;

        FieldInfo field = typeof(PlayerInput).GetField(fieldName, InstanceFields);
        return field != null ? field.GetValue(keybinds) as Array : null;
    }

    private static string[] GetOptions(string inputName, object entry)
    {
        if (entry is string)
            return ButtonDevices;
        if (GetMember(entry, "useComposite") is bool)
        {
            string normalized = Normalize(inputName);
            if (normalized == "movehand")
                return Vector3Devices;
            return Vector2Devices;
        }
        if (GetMember(entry, "binding") is string)
            return ButtonDevices;
        return Array.Empty<string>();
    }

    private static string GetCurrentType(object entry)
    {
        string mode = GetMember(entry, "mode")?.ToString();
        if (string.Equals(mode, "Scroll", StringComparison.OrdinalIgnoreCase))
            return "Scroll";
        if (string.Equals(mode, "Tilt", StringComparison.OrdinalIgnoreCase))
            return "Motion";
        if (GetMember(entry, "useComposite") is bool composite && composite)
            foreach (string part in GetParts(entry))
            {
                string path = GetMember(entry, part) as string;
                string type = GetTypeFromPath(path);
                if (type != null)
                    return type;
            }

        string binding = entry as string ?? GetMember(entry, "binding") as string;
        return GetTypeFromPath(binding) ?? "Keyboard";
    }

    private static bool ApplyDeviceType(
        PlayerInput keybinds,
        string inputName,
        int bindingIndex,
        string selectedType)
    {
        Array bindings = GetBindings(keybinds, inputName);
        if (bindings == null || bindingIndex < 0 || bindingIndex >= bindings.Length)
            return false;

        object entry = bindings.GetValue(bindingIndex);
        if (entry == null)
            return false;
        if (selectedType == "Scroll" || selectedType == "Motion")
        {
            if (entry is string)
                return false;
            if (!SetEnumMember(entry, "mode", selectedType == "Scroll" ? "Scroll" : "Tilt"))
                return false;

            SetMember(entry, "useComposite", false);
            if (selectedType == "Scroll")
                SetMember(entry, "binding", "<Mouse>/scroll");
        }
        else if (entry is string path)
            entry = ReplaceDevice(path, selectedType);
        else
        {
            bool composite = GetMember(entry, "useComposite") is bool value && value;
            if (composite)
                foreach (string part in GetParts(entry))
                {
                    string component = GetMember(entry, part) as string;
                    if (!string.IsNullOrWhiteSpace(component))
                        SetMember(entry, part,
                            ReplaceDevice(component, selectedType));
                }
            else
            {
                string bindingPath = GetMember(entry, "binding") as string;
                if (string.IsNullOrWhiteSpace(bindingPath))
                    return false;
                SetMember(entry, "binding", ReplaceDevice(bindingPath, selectedType));
            }

            string mode = GetMember(entry, "mode")?.ToString();
            if (mode == "Scroll" || mode == "Tilt")
                SetEnumMember(entry, "mode", "Direction");
        }

        bindings.SetValue(entry, bindingIndex);
        return true;
    }

    private static string ReplaceDevice(string path, string device)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        int end = path.IndexOf('>');
        if (!path.StartsWith("<", StringComparison.Ordinal) || end < 0 || end + 1 >= path.Length || path[end + 1] != '/')
            return path;

        string control = path.Substring(end + 2);
        string layout = device switch
        {
            "Keyboard" => "Keyboard",
            "Mouse" => "Mouse",
            "Gamepad" => "Gamepad",
            _ => null
        };
        return layout == null ? path : $"<{layout}>/{control}";
    }

    private static string GetTypeFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        int end = path.IndexOf('>');
        if (!path.StartsWith("<", StringComparison.Ordinal) || end < 0)
            return null;

        string device = path.Substring(1, end - 1);
        string control = path.Substring(end + 1);
        if (control.IndexOf("/scroll", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Scroll";
        if (device.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
            return "Keyboard";
        if (device.Equals("Mouse", StringComparison.OrdinalIgnoreCase))
            return "Mouse";
        if (device.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
            return "Gamepad";
        if (device.Equals("Sensor", StringComparison.OrdinalIgnoreCase))
            return "Motion";
        return null;
    }

    private static string[] GetParts(object entry)
    {
        return entry.GetType().GetField("forward", InstanceFields) != null
            ? new[] { "up", "down", "left", "right", "forward", "backward" }
            : new[] { "up", "down", "left", "right" };
    }

    private static object GetMember(object target, string name)
    {
        if (target == null)
            return null;

        FieldInfo field = target.GetType().GetField(name, InstanceFields);
        if (field != null)
            return field.GetValue(target);

        PropertyInfo property = target.GetType().GetProperty(name, InstanceFields);
        return property != null && property.CanRead
            ? property.GetValue(target) : null;
    }

    private static bool SetMember(object target, string name, object value)
    {
        if (target == null)
            return false;

        FieldInfo field = target.GetType().GetField(name, InstanceFields);
        if (field != null)
        {
            field.SetValue(target, value);
            return true;
        }

        PropertyInfo property = target.GetType().GetProperty(name, InstanceFields);
        if (property == null || !property.CanWrite)
            return false;

        property.SetValue(target, value);
        return true;
    }

    private static bool SetEnumMember(
        object target, string name, string enumValue)
    {
        FieldInfo field = target?.GetType().GetField(name, InstanceFields);
        if (field == null || !field.FieldType.IsEnum)
            return false;

        object value = Enum.Parse(field.FieldType, enumValue, true);
        field.SetValue(target, value);
        return true;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        System.Text.StringBuilder result = new(value.Length);
        foreach (char character in value)
            if (char.IsLetterOrDigit(character))
                result.Append(char.ToLowerInvariant(character));
        return result.ToString();
    }
}
