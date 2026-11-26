using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuLabel : VisualElement
{
    private TextElement titleElement;
    

    [UxmlAttribute]
    public string title
    {
        get => titleElement.text;
        set => titleElement.text = value;
    }

    public MenuLabel()
    {
        AddToClassList("menu-label");

        TextElement title = new();
        hierarchy.Add(title);

        title.text = "Title";
        title.AddToClassList("menu-label__title");
        titleElement = title;
    }
}
