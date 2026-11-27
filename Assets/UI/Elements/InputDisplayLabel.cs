using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class InputDisplayLabel : VisualElement
{
    private readonly Label textLabel;
    private readonly Image keybindIcon;

    private PlayerInput playerInput;
    private string inputName = string.Empty;
    private bool fillHeight;
    private bool isSubscribed;
    private IVisualElementScheduledItem holdProgressUpdate;

    private bool hasOriginalPadding;
    private float originalPaddingTop;
    private float originalPaddingBottom;
    private float originalPaddingLeft;
    private float originalPaddingRight;


    [UxmlAttribute]
    public string text
    {
        get => textLabel.text;
        set => textLabel.text = value ?? string.Empty;
    }

    [UxmlAttribute]
    public string InputName
    {
        get => inputName;
        set
        {
            if (inputName == value)
                return;
            inputName = value;
            RefreshIcon();
        }
    }

    [UxmlAttribute]
    public bool FillHeight
    {
        get => fillHeight;
        set
        {
            fillHeight = value;
            style.height = fillHeight
                ? Length.Percent(100) : StyleKeyword.Auto;
        }
    }

    public PlayerInput Input
    {
        get => playerInput;
        set
        {
            if (playerInput == value)
                return;
            UnsubscribeFromInput();
            playerInput = value;
            SubscribeToInput();
            RefreshIcon();
        }
    }


    public InputDisplayLabel()
    {
        AddToClassList("input-display-label");

        textLabel = new("Label");
        textLabel.AddToClassList("input-display-label__text");

        keybindIcon = new() { scaleMode = ScaleMode.ScaleToFit };
        keybindIcon.AddToClassList("input-display-label__icon");

        hierarchy.Add(textLabel);
        hierarchy.Add(keybindIcon);

        style.height = StyleKeyword.Auto;

        RegisterCallback<AttachToPanelEvent>(_ => SubscribeToInput());
        RegisterCallback<DetachFromPanelEvent>(_ => UnsubscribeFromInput());

        RegisterCallback<AttachToPanelEvent>(_ => {
            holdProgressUpdate = schedule
                .Execute(UpdateHoldBorder)
                .Every(16);
        });
        RegisterCallback<DetachFromPanelEvent>(_ =>
        {
            holdProgressUpdate?.Pause();
            holdProgressUpdate = null;
        });

        RegisterCallback<GeometryChangedEvent>(CaptureOriginalPadding);
    }


    private void SubscribeToInput()
    {
        if (isSubscribed || playerInput == null || panel == null)
            return;
        playerInput.ActiveDeviceChanged += OnActiveDeviceChanged;
        isSubscribed = true;
    }

    private void UnsubscribeFromInput()
    {
        if (!isSubscribed || playerInput == null)
            return;
        playerInput.ActiveDeviceChanged -= OnActiveDeviceChanged;
        isSubscribed = false;
    }

    public void OnActiveDeviceChanged()
        => RefreshIcon();
    
    private void UpdateHoldBorder()
    {
        HoldableButtonInput holdable = playerInput?.GetHoldableButtonInput(inputName);
        float progress = holdable != null
            ? holdable.HoldProgress : 0.0f;
        float width = progress >= 1.0f
            ? 0.0f : progress * 10.0f;

        if (hasOriginalPadding)
        {
            style.paddingTop = Mathf.Max(0f, originalPaddingTop - width);
            style.paddingBottom = Mathf.Max(0f, originalPaddingBottom - width);
            style.paddingLeft = Mathf.Max(0f, originalPaddingLeft - width);
            style.paddingRight = Mathf.Max(0f, originalPaddingRight - width);
        }

        style.borderTopWidth = width;
        style.borderBottomWidth = width;
        style.borderLeftWidth = width;
        style.borderRightWidth = width;
    }

    private void CaptureOriginalPadding(GeometryChangedEvent evt)
    {
        originalPaddingTop = resolvedStyle.paddingTop;
        originalPaddingBottom = resolvedStyle.paddingBottom;
        originalPaddingLeft = resolvedStyle.paddingLeft;
        originalPaddingRight = resolvedStyle.paddingRight;

        hasOriginalPadding = true;
        UnregisterCallback<GeometryChangedEvent>(CaptureOriginalPadding);
    }
    
    private void RefreshIcon()
    {
        if (playerInput == null || string.IsNullOrWhiteSpace(inputName))
        {
            SetIcon(null);
            return;
        }

        InputBindingDisplayData[] bindings = playerInput.GetBindingDisplayData(inputName);
        if (bindings == null || bindings.Length == 0)
        {
            SetIcon(null);
            return;
        }

        string activeDevice = playerInput.ActiveDeviceName;
        InputBindingDisplayData selected = FindBindingForDevice(bindings, activeDevice);
        if (selected == null)
        {
            string fallbackDevice = null;
            if (string.Equals(activeDevice, "Keyboard", StringComparison.OrdinalIgnoreCase))
                fallbackDevice = "Mouse";
            else if (string.Equals(activeDevice, "Mouse", StringComparison.OrdinalIgnoreCase))
                fallbackDevice = "Keyboard";
            selected = FindBindingForDevice(bindings, fallbackDevice);
        }
        selected ??= FindFirstIconBinding(bindings);
        SetIcon(selected?.Icon);
    }

    private static InputBindingDisplayData FindBindingForDevice(InputBindingDisplayData[] bindings, string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            return null;
        foreach (InputBindingDisplayData binding in bindings)
            if (
                binding != null &&
                binding.VisualType == InputBindingVisualType.Icon &&
                string.Equals(binding.ControlName, deviceName, StringComparison.OrdinalIgnoreCase)
            )
                return binding;
        return null;
    }

    private static InputBindingDisplayData FindFirstIconBinding(InputBindingDisplayData[] bindings)
    {
        foreach (InputBindingDisplayData binding in bindings)
            if (binding != null && binding.VisualType == InputBindingVisualType.Icon)
                return binding;
        return null;
    }

    private void SetIcon(UnityEngine.Object source)
    {
        keybindIcon.image = source as Texture2D;
        keybindIcon.vectorImage = source as VectorImage;
    }
}
