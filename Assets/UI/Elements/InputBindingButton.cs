using System;
using UnityEngine.UIElements;

public enum InputBindingVisualType
{
    Key,
    KeyCluster4,
    KeyCluster6,
    MouseDelta,
    MouseButton,
    ScrollWheel,
    Tilt,
    GenericIcon
}

public class InputBindingDisplayData
{
    public string ControlName { get; set; } = string.Empty;
    public InputBindingVisualType VisualType { get; set; }
    public string[] KeyLabels { get; set; } = Array.Empty<string>();
    public string IconClassName { get; set; } = string.Empty;
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
            case InputBindingVisualType.Key:
                visualContainer.Add(CreateKey(data.KeyLabels, 0));
                break;
            case InputBindingVisualType.KeyCluster4:
                CreateKeyCluster4(data.KeyLabels);
                break;
            case InputBindingVisualType.KeyCluster6:
                CreateKeyCluster6(data.KeyLabels);
                break;
            default:
                CreateIcon(data);
                break;
        }
    }

    private void CreateKeyCluster4(string[] labels)
    {
        VisualElement cluster = CreateKeyCluster();

        VisualElement topRow = CreateKeyRow();
        topRow.Add(CreateKeyPlaceholder());
        topRow.Add(CreateKey(labels, 0));
        topRow.Add(CreateKeyPlaceholder());

        VisualElement bottomRow = CreateKeyRow(true);
        bottomRow.Add(CreateKey(labels, 1));
        bottomRow.Add(CreateKey(labels, 2));
        bottomRow.Add(CreateKey(labels, 3));

        cluster.Add(topRow);
        cluster.Add(bottomRow);

        visualContainer.Add(cluster);
    }

    private void CreateKeyCluster6(string[] labels)
    {
        VisualElement cluster = CreateKeyCluster();
        for (int rowIndex = 0; rowIndex < 2; rowIndex++)
        {
            VisualElement row = CreateKeyRow(rowIndex > 0);
            for (int columnIndex = 0; columnIndex < 3; columnIndex++)
            {
                int keyIndex = rowIndex * 3 + columnIndex;
                row.Add(CreateKey(labels, keyIndex));
            }
            cluster.Add(row);
        }
        visualContainer.Add(cluster);
    }

    private void CreateIcon(InputBindingDisplayData data)
    {
        VisualElement icon = new();
        icon.AddToClassList("input-binding-button__icon");
        icon.AddToClassList(GetIconModifier(data.VisualType));
        if (!string.IsNullOrWhiteSpace(data.IconClassName))
            icon.AddToClassList(data.IconClassName);
        visualContainer.Add(icon);
    }

    private static string GetIconModifier(InputBindingVisualType type)
        => type switch
        {
            InputBindingVisualType.MouseDelta => "input-binding-button__icon--mouse-delta",
            InputBindingVisualType.MouseButton => "input-binding-button__icon--mouse-button",
            InputBindingVisualType.ScrollWheel => "input-binding-button__icon--scroll-wheel",
            InputBindingVisualType.Tilt => "input-binding-button__icon--tilt",
            _ => "input-binding-button__icon--generic"
        };

    private static VisualElement CreateKeyCluster()
    {
        VisualElement cluster = new();
        cluster.AddToClassList("input-binding-button__key-cluster");
        return cluster;
    }

    private static VisualElement CreateKeyRow(bool secondRow = false)
    {
        VisualElement row = new();
        row.AddToClassList("input-binding-button__key-row");
        if (secondRow)
            row.AddToClassList("input-binding-button__key-row--second");
        return row;
    }

    private static Label CreateKey(string[] labels, int index)
    {
        string text = labels != null && index < labels.Length
            ? labels[index] ?? string.Empty : string.Empty;
        Label key = new(text);
        key.AddToClassList("input-binding-button__key");
        return key;
    }

    private static VisualElement CreateKeyPlaceholder()
    {
        VisualElement placeholder = new();
        placeholder.AddToClassList("input-binding-button__key-placeholder");
        return placeholder;
    }
}
