using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum InputBindingVisualType
{
    Icon,
    IconCluster4,
    IconCluster6
}

public class InputBindingDisplayData
{
    public string ControlName { get; set; } = string.Empty;

    public InputBindingVisualType VisualType { get; set; }

    public string IconName { get; set; } = "undefined";

    public string[] IconNames { get; set; } = Array.Empty<string>();

    public Texture2D Icon { get; set; }

    public Texture2D[] Icons { get; set; } = Array.Empty<Texture2D>();
}

[UxmlElement]
public partial class InputBindingButton : Button
{
    private readonly Label controlLabel;
    private readonly VisualElement visualContainer;

    public InputBindingButton()
    {
        AddToClassList("input-binding-button");

        controlLabel = new();
        controlLabel.AddToClassList("input-binding-button__name");

        visualContainer = new();
        visualContainer.AddToClassList("input-binding-button__visual");

        VisualElement outline = new();
        outline.AddToClassList("input-binding-button__outline");
        outline.pickingMode = PickingMode.Ignore;

        Add(controlLabel);
        Add(visualContainer);
        Add(outline);
    }


    public void SetDisplayData(InputBindingDisplayData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        controlLabel.text = data.ControlName;
        visualContainer.Clear();
        switch (data.VisualType)
        {
            case InputBindingVisualType.Icon:
                visualContainer.Add(CreateIcon(data.Icon));
                break;
            case InputBindingVisualType.IconCluster4:
                CreateIconCluster4(data.Icons);
                break;
            case InputBindingVisualType.IconCluster6:
                CreateIconCluster6(data.Icons);
                break;
        }
    }

    private void CreateIconCluster4(Texture2D[] icons)
    {
        VisualElement cluster = CreateIconCluster();

        VisualElement topRow = CreateIconRow();
        topRow.Add(CreateIconPlaceholder());
        topRow.Add(CreateIconAt(icons, 0));
        topRow.Add(CreateIconPlaceholder());

        VisualElement bottomRow = CreateIconRow(true);
        bottomRow.Add(CreateIconAt(icons, 1));
        bottomRow.Add(CreateIconAt(icons, 2));
        bottomRow.Add(CreateIconAt(icons, 3));

        cluster.Add(topRow);
        cluster.Add(bottomRow);

        visualContainer.Add(cluster);
    }

    private void CreateIconCluster6(Texture2D[] icons)
    {
        VisualElement cluster = CreateIconCluster();

        VisualElement topRow = CreateIconRow();
        topRow.Add(CreateIconAt(icons, 4));
        topRow.Add(CreateIconAt(icons, 0));
        topRow.Add(CreateIconAt(icons, 5));

        VisualElement bottomRow = CreateIconRow(true);
        bottomRow.Add(CreateIconAt(icons, 2));
        bottomRow.Add(CreateIconAt(icons, 1));
        bottomRow.Add(CreateIconAt(icons, 3));

        cluster.Add(topRow);
        cluster.Add(bottomRow);

        visualContainer.Add(cluster);
    }

    private static Image CreateIcon(Texture2D texture)
    {
        Image icon = new()
        {
            image = texture,
            scaleMode = ScaleMode.ScaleToFit
        };
        icon.AddToClassList("input-binding-button__icon");
        return icon;
    }

    private static Image CreateIconAt(Texture2D[] icons, int index)
    {
        Texture2D texture = icons != null && index < icons.Length
            ? icons[index] : null;
        return CreateIcon(texture);
    }

    private static VisualElement CreateIconCluster()
    {
        VisualElement cluster = new();
        cluster.AddToClassList("input-binding-button__icon-cluster");
        return cluster;
    }

    private static VisualElement CreateIconRow(bool secondRow = false)
    {
        VisualElement row = new();
        row.AddToClassList("input-binding-button__icon-row");
        if (secondRow)
            row.AddToClassList("input-binding-button__icon-row--second");
        return row;
    }

    private static VisualElement CreateIconPlaceholder()
    {
        VisualElement placeholder = new();
        placeholder.AddToClassList("input-binding-button__icon-placeholder");
        return placeholder;
    }
}
