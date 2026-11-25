using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuFoldout : VisualElement
{
    private readonly MenuButton header;
    private readonly VisualElement content;
    private bool isExpanded;

    public override VisualElement contentContainer => content;


    [UxmlAttribute]
    public string text
    {
        get => header.text;
        set => header.text = value;
    }

    [UxmlAttribute]
    public bool expanded
    {
        get => isExpanded;
        set => SetExpanded(value);
    }

    public MenuFoldout()
    {
        AddToClassList("menu-foldout");

        header = new MenuButton { name = "Header" };
        header.AddToClassList("menu-foldout__header");
        header.clicked += ToggleExpanded;
        hierarchy.Add(header);

        content = new() { name = "Content" };
        content.AddToClassList("menu-foldout__content");
        hierarchy.Add(content);

        SetExpanded(false);
    }


    private void ToggleExpanded()
        => expanded = !expanded;

    private void SetExpanded(bool value)
    {
        isExpanded = value;
        content.style.display = value
            ? DisplayStyle.Flex : DisplayStyle.None;
        if (value)
            header.AddToClassList("menu-button--active");
        else
            header.RemoveFromClassList("menu-button--active");
    }
}
