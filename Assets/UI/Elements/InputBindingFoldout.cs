
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
        for (int i = 0; i < bindings.Length; i++)
        {
            InputBindingDisplayData binding = bindings[i];
            if (binding == null)
                continue;

            InputBindingButton button = new();
            button.SetDisplayData(binding);

            PlayerInput capturedInput = playerInput;
            string capturedInputName = inputName;
            int capturedIndex = i;
            string capturedTitle = binding.ControlName;

            button.clicked += () => KeybindPopupController.Show(
                capturedInput,
                capturedInputName,
                capturedIndex,
                capturedTitle,
                PopupContentAsset,
                PopupRowAsset,
                RefreshBindings
            );

            contentContainer.Add(button);
        }
    }
}
