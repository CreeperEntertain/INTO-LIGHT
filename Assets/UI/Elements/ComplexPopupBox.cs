using UnityEngine.UIElements;

[UxmlElement(libraryPath = "Popups")]
public partial class ComplexPopupBox : VisualElement
{
    private readonly VisualElement popupPanel;
    private readonly Label titleLabel;
    private readonly ScrollView scrollView;
    private readonly VisualElement content;
    private readonly VisualElement buttonContainer;


    public VisualElement Content => content;
    public bool IsOpen => style.display == DisplayStyle.Flex;

    public ComplexPopupBox()
    {
        AddToClassList("popup-box");
        RegisterCallback<ClickEvent>(OnOverlayClicked);

        popupPanel = new();
        popupPanel.AddToClassList("popup-box__panel");
        hierarchy.Add(popupPanel);

        titleLabel = new();
        titleLabel.AddToClassList("popup-box__panel");
        popupPanel.Add(titleLabel);

        scrollView = new(ScrollViewMode.Vertical);
        scrollView.AddToClassList("popup-box__scroll-view");
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        popupPanel.Add(scrollView);

        content = new();
        content.AddToClassList("popup-box__content");
        scrollView.Add(content);

        buttonContainer = new();
        buttonContainer.AddToClassList("popup-box__buttons");
        popupPanel.Add(buttonContainer);

        style.display = DisplayStyle.None;
    }

    public void Show(string title, string description, params PopupButton[] buttons)
    {
        titleLabel.text = title ?? string.Empty;
        content.Clear();

        Label descriptionLabel = new(description ?? string.Empty);
        descriptionLabel.AddToClassList("popup-box__description");
        content.Add(descriptionLabel);

        ShowButtons(buttons);
        Open();
    }

    public void Show(string title, VisualElement element, params PopupButton[] buttons)
    {
        titleLabel.text = title ?? string.Empty;
        content.Clear();
        if (element != null)
        {
            element.RemoveFromHierarchy();
            content.Add(element);
        }
        ShowButtons(buttons);
        Open();
    }

    public void Show(string title, UIDocument sourceDocument, params PopupButton[] buttons)
        => Show(title, sourceDocument != null ? sourceDocument.rootVisualElement : null, buttons);

    private void ShowButtons(PopupButton[] buttons)
    {
        buttonContainer.Clear();
        if (buttons == null)
            return;
        foreach (PopupButton definition in buttons)
        {
            if (definition == null)
                continue;
            Button button = new() { text = definition.Text ?? string.Empty };
            button.AddToClassList("popup-box__button");
            button.clicked += () =>
            {
                definition.OnClick?.Invoke();
                Hide();
            };
            buttonContainer.Add(button);
        }
    }

    private void Open()
    {
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
