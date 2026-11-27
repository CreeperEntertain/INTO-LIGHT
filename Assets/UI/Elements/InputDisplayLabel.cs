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
        => RefreshIcon(allowFallback: false);

    public void RefreshIcon()
        => RefreshIcon(allowFallback: true);
    
    private void RefreshIcon(bool allowFallback)
    {
        if (playerInput == null || string.IsNullOrWhiteSpace(inputName))
        {
            if (allowFallback)
                SetIcon(null);
            return;
        }

        InputBindingDisplayData[] bindings = playerInput.GetBindingDisplayData(inputName);
        if (bindings == null || bindings.Length == 0)
        {
            if (allowFallback)
                SetIcon(null);
            return;
        }

        InputBindingDisplayData selected = null;
        string activeDevice = playerInput.ActiveDeviceName;
        if (!string.IsNullOrEmpty(activeDevice))
            foreach (InputBindingDisplayData binding in bindings)
            {
                if (binding == null || binding.VisualType != InputBindingVisualType.Icon)
                    continue;
                if (string.Equals(binding.ControlName, activeDevice, StringComparison.OrdinalIgnoreCase))
                {
                    selected = binding;
                    break;
                }
            }
        if (selected == null && allowFallback)
            foreach (InputBindingDisplayData binding in bindings)
                if (binding != null && binding.VisualType == InputBindingVisualType.Icon)
                {
                    selected = binding;
                    break;
                }
        if (selected != null)
            SetIcon(selected.Icon);
        else if (allowFallback)
            SetIcon(null);
    }

    private void SetIcon(UnityEngine.Object source)
    {
        keybindIcon.image = source as Texture2D;
        keybindIcon.vectorImage = source as VectorImage;
    }
}
