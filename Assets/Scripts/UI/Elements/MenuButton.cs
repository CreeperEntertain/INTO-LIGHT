using UnityEngine;
using UnityEngine.UIElements;

public class MenuButton : Button
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
