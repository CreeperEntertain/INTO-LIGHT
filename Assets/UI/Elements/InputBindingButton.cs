using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum InputBindingVisualType
{
    Icon,
    IconCluster4,
    IconCluster6
}

public enum InputBindingValueType
{
    Button,
    Vector2,
    Vector3
}

public sealed class InputBindingTarget
{
    public string InputName { get; set; }
    public int SourceIndex { get; set; }
    public string ComponentName { get; set; }
    public string CurrentPath { get; set; }
    public InputBindingValueType ValueType { get; set; }
    public Action<string> ApplyPath { get; set; }
}

public sealed class InputBindingOption
{
    public string Label { get; set; }
    public string Path { get; set; }
}

public class InputBindingDisplayData
{
    public string ControlName { get; set; } = string.Empty;
    public InputBindingVisualType VisualType { get; set; }
    public string IconName { get; set; } = "empty";
    public string[] IconNames { get; set; } = Array.Empty<string>();
    public UnityEngine.Object Icon { get; set; }
    public UnityEngine.Object EmptyIcon { get; set; }
    public UnityEngine.Object[] Icons { get; set; } = Array.Empty<Texture2D>();
    public InputBindingTarget[] BindingTargets { get; set; } = Array.Empty<InputBindingTarget>();
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
                CreateIconCluster4(data.Icons, data.EmptyIcon);
                break;
            case InputBindingVisualType.IconCluster6:
                CreateIconCluster6(data.Icons);
                break;
        }
    }

    private void CreateIconCluster4(
        UnityEngine.Object[] icons,
        UnityEngine.Object emptyIcon)
    {
        VisualElement cluster = CreateIconCluster();

        VisualElement topRow = CreateIconRow();
        topRow.Add(CreateIconPlaceholder(emptyIcon));
        topRow.Add(CreateIconAt(icons, 0));
        topRow.Add(CreateIconPlaceholder(emptyIcon));

        VisualElement bottomRow = CreateIconRow(true);
        bottomRow.Add(CreateIconAt(icons, 1));
        bottomRow.Add(CreateIconAt(icons, 2));
        bottomRow.Add(CreateIconAt(icons, 3));

        cluster.Add(topRow);
        cluster.Add(bottomRow);

        visualContainer.Add(cluster);
    }

    private void CreateIconCluster6(UnityEngine.Object[] icons)
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

    private static Image CreateIcon(UnityEngine.Object source)
    {
        Image icon = new() { scaleMode = ScaleMode.ScaleToFit };
        if (source is Texture2D texture)
            icon.image = texture;
        else if (source is VectorImage vectorImage)
            icon.vectorImage = vectorImage;
        icon.AddToClassList("input-binding-button__icon");
        return icon;
    }

    private static Image CreateIconAt(UnityEngine.Object[] icons, int index)
    {
        UnityEngine.Object source = icons != null && index < icons.Length
            ? icons[index] : null;
        return CreateIcon(source);
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

    private static Image CreateIconPlaceholder(UnityEngine.Object source)
    {
        Image placeholder = CreateIcon(source);
        placeholder.RemoveFromClassList("input-binding-button__icon");
        placeholder.AddToClassList("input-binding-button__icon-placeholder");
        return placeholder;
    }
}
