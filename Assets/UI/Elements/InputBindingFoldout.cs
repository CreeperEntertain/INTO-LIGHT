using UnityEngine.UIElements;

[UxmlElement]
public partial class InputBindingFoldout : MenuFoldout
{
    private PlayerInput playerInput;
    private string inputName = string.Empty;


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
        bool firstButton = true;
        foreach (InputBindingDisplayData binding in bindings)
        {
            if (binding == null)
                return;
            InputBindingButton button = new();
            if (firstButton)
                firstButton = false;
            else
                button.style.marginTop = 20;
            button.SetDisplayData(binding);
            contentContainer.Add(button);
        }
    }
}
