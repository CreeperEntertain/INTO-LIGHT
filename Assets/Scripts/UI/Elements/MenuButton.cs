using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuButton : Button
{
    private readonly VisualElement stripe;
    private readonly TextElement invertedText;

    public MenuButton()
    {
        AddToClassList("menu-button");

        var stripe = new VisualElement();
        stripe.AddToClassList("menu-button__stripe");
        stripe.pickingMode = PickingMode.Ignore;

        invertedText = new();
        invertedText.AddToClassList("menu-button__inverted-text");
        invertedText.pickingMode = PickingMode.Ignore;

        stripe.Add(invertedText);
        hierarchy.Add(stripe);

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        RegisterCallback<ChangeEvent<string>>(OnTextChanged);
    }

    private void OnAttachToPanel(AttachToPanelEvent evt)
    {
        if (evt.target != this)
            return;
        SyncText();
        SyncTextLayout(resolvedStyle.width);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (evt.target != this)
            return;
        SyncTextLayout(evt.newRect.width);
    }

    private void OnTextChanged(ChangeEvent<string> evt)
    {
        if (evt.target != this)
            return;
        invertedText.text = evt.newValue;
    }

    private void SyncText()
    {
        invertedText.text = text;
        invertedText.enableRichText = enableRichText;
        invertedText.emojiFallbackSupport = emojiFallbackSupport;
        invertedText.parseEscapeSequences = parseEscapeSequences;
    }

    private void SyncTextLayout(float width)
    {
        invertedText.style.width = width;

        invertedText.style.paddingLeft = resolvedStyle.paddingLeft;
        invertedText.style.paddingRight = resolvedStyle.paddingRight;
        invertedText.style.paddingTop = resolvedStyle.paddingTop;
        invertedText.style.paddingBottom = resolvedStyle.paddingBottom;
    }
}
