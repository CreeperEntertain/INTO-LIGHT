using UnityEngine.UIElements;

[UxmlElement]
public partial class MenuButton : Button
{
    private readonly TextElement invertedText;

    private bool textLayoutSyncQueued;

    public MenuButton()
    {
        AddToClassList("menu-button");

        VisualElement stripe = new();
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
        QueueTextLayoutSync();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (evt.target != this)
            return;
        QueueTextLayoutSync();
    }

    private void OnTextChanged(ChangeEvent<string> evt)
    {
        if (evt.target != this)
            return;
        invertedText.text = evt.newValue;
    }

    private void QueueTextLayoutSync()
    {
        if (textLayoutSyncQueued)
            return;

        textLayoutSyncQueued = true;
        schedule.Execute(() =>
        {
            textLayoutSyncQueued = false;
            if (panel == null)
                return;

            SyncTextLayout(resolvedStyle.width, resolvedStyle.height);
        }).ExecuteLater(0);
    }

    private void SyncText()
    {
        invertedText.text = text;
        invertedText.enableRichText = enableRichText;
        invertedText.emojiFallbackSupport = emojiFallbackSupport;
        invertedText.parseEscapeSequences = parseEscapeSequences;
    }

    private void SyncTextLayout(float width, float height)
    {
        invertedText.style.left = 0;
        invertedText.style.top = 0;
        invertedText.style.width = width;
        invertedText.style.height = height;

        invertedText.style.paddingLeft = resolvedStyle.paddingLeft;
        invertedText.style.paddingRight = resolvedStyle.paddingRight;
        invertedText.style.paddingTop = resolvedStyle.paddingTop;
        invertedText.style.paddingBottom = resolvedStyle.paddingBottom;
    }
}
