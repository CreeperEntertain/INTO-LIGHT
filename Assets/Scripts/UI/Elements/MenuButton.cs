using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuButton : Button
{
    public MenuButton()
    {
        AddToClassList("menu-button");

        var stripe = new VisualElement();
        stripe.AddToClassList("menu-button__stripe");
        stripe.pickingMode = PickingMode.Ignore;

        hierarchy.Add(stripe);
    }
}
