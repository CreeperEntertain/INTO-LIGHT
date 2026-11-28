using UnityEngine.UIElements;

[UxmlElement]
public partial class InputBindingFoldout : MenuFoldout
{
    private PlayerInput playerInput;
    private string inputName = string.Empty;

    public VisualTreeAsset PopupContentAsset { get; set; }
    public VisualTreeAsset PopupRowAsset { get; set; }


    [UxmlAttribute]
    public string InputName
    {
        get => inputName;
        set
        {
            if (inputName == value)
                return;
            inputName = value;
            RefreshBindings();
        }
    }

    public PlayerInput Input
    {
        get => playerInput;
        set
        {
            if (playerInput == value)
                return;
            playerInput = value;
            RefreshBindings();
        }
    }

    public InputBindingFoldout()
    {
        AddToClassList("input-binding-foldout");
    }


    public void RefreshBindings()
    {
        contentContainer.Clear();
        if (playerInput == null || string.IsNullOrWhiteSpace(inputName))
            return;
        
        InputBindingDisplayData[] bindings = playerInput.GetBindingDisplayData(inputName);
        if (bindings == null)
            return;
        foreach (InputBindingDisplayData binding in bindings)
        {
            if (binding == null)
                return;
            InputBindingButton button = new();
            button.SetDisplayData(binding);
            if (binding.BindingTargets != null && binding.BindingTargets.Length > 0)
            {
                InputBindingDisplayData capturedData = binding;
                button.clicked += () => KeybindPopupController.Show(playerInput, capturedData, PopupContentAsset, PopupRowAsset, RefreshBindings);
            }

            contentContainer.Add(button);
        }
    }
}
