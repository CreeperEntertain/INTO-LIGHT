using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuFoldout : VisualElement
{
    private readonly MenuButton header;
    private readonly VisualElement contentClip;
    private readonly GroupBox content;

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

        contentClip = new() { name = "ContentClip" };
        contentClip.AddToClassList("menu-foldout__content-clip");
        hierarchy.Add(contentClip);

        content = new() { name = "Content" };
        content.AddToClassList("menu-foldout__content");
        contentClip.Add(content);

        content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);

        SetExpanded(false);
    }


    private void ToggleExpanded()
        => expanded = !expanded;

    private void SetExpanded(bool value)
    {
        isExpanded = value;
        if (value)
        {
            header.AddToClassList("menu-button--active");
            contentClip.style.height = content.resolvedStyle.height;
        }
        else
        {
            header.RemoveFromClassList("menu-button--active");
            contentClip.style.height = 0;
        }
    }

    private void OnContentGeometryChanged(GeometryChangedEvent evt)
    {
        if (!isExpanded)
            return;
        
        float height = content.resolvedStyle.height;
        if (!float.IsNaN(height))
            contentClip.style.height = height;
    }
}
