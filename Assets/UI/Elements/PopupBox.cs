using UnityEngine.UIElements;

[UxmlElement]
public partial class PopupBox : VisualElement
{
    private readonly VisualElement popupPanel;
    private readonly Label titleLabel;
    private readonly Label descriptionLabel;
    private readonly ScrollView scrollView;
    private readonly VisualElement content;
    private readonly VisualElement buttonContainer;

    public VisualElement Content => content;

    public PopupBox()
    {
        AddToClassList("popup-box");

        RegisterCallback<ClickEvent>(OnOverlayClicked);

        popupPanel = new();
        popupPanel.AddToClassList("popup-box__panel");
        hierarchy.Add(popupPanel);

        titleLabel = new();
        titleLabel.AddToClassList("popup-box__title");
        popupPanel.Add(titleLabel);

        scrollView = new(ScrollViewMode.Vertical);
        scrollView.AddToClassList("popup-box__scroll-view");
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        popupPanel.Add(scrollView);

        content = new();
        content.AddToClassList("popup-box__content");
        scrollView.Add(content);

        descriptionLabel = new();
        descriptionLabel.AddToClassList("popup-box__description");
        content.Add(descriptionLabel);

        buttonContainer = new();
        buttonContainer.AddToClassList("popup-box__buttons");
        popupPanel.Add(buttonContainer);

        style.display = DisplayStyle.None;
    }

    public void Show(string title, string description, params PopupButton[] buttons)
    {
        titleLabel.text = title ?? string.Empty;
        descriptionLabel.text = description ?? string.Empty;

        content.Clear();
        content.Add(descriptionLabel);

        buttonContainer.Clear();
        if (buttons != null)
            foreach (PopupButton definition in buttons)
            {
                if (definition == null)
                    continue;
                PopupButton buttonDefinition = definition;
                Button button = new() { text = buttonDefinition.Text ?? string.Empty };
                button.AddToClassList("popup-box__button");
                button.clicked += () =>
                {
                    buttonDefinition.OnClick?.Invoke();
                    Hide();
                };
                buttonContainer.Add(button);
            }
        
        style.display = DisplayStyle.Flex;
        BringToFront();
    }

    public void Hide()
        => style.display = DisplayStyle.None;
    
    private void OnOverlayClicked(ClickEvent evt)
    {
        if (evt.target == this)
            Hide();
    }
}
