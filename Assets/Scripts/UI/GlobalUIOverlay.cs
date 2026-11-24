using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GlobalUIOverlay : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private VisualElement marker;
    private Vector2 cursorPosition;


    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();

        document.panelSettings.sortingOrder = 10000f;
        document.sortingOrder = 10000f;

        VisualElement root = document.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;
        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);

        cursorPosition = new(Screen.width / 2.0f, Screen.height / 2.0f);

        marker = new VisualElement
        {
            name = "OverlayMarker",
            pickingMode = PickingMode.Ignore
        };

        marker.style.position = Position.Absolute;
        marker.style.left = 0;
        marker.style.top = 0;
        marker.style.width = 24;
        marker.style.height = 24;
        marker.style.flexShrink = 0;
        marker.style.backgroundColor = new Color(0f, 0f, 0f, 0.85f);

        marker.style.borderTopWidth = 1;
        marker.style.borderBottomWidth = 1;
        marker.style.borderLeftWidth = 1;
        marker.style.borderRightWidth = 1;

        marker.style.borderTopColor = Color.white;
        marker.style.borderBottomColor = Color.white;
        marker.style.borderLeftColor = Color.white;
        marker.style.borderRightColor = Color.white;

        marker.style.borderTopLeftRadius = 12;
        marker.style.borderTopRightRadius = 12;
        marker.style.borderBottomLeftRadius = 12;
        marker.style.borderBottomRightRadius = 12;

        root.Add(marker);
        marker.BringToFront();
    }

    private void Update()
    {
        if (marker == null || marker.panel == null || playerInput?.MoveCursor == null)
            return;
        
        Vector2 movement = playerInput.MoveCursor.Value;
        if (playerInput.MoveCursor.IsRelative)
            cursorPosition += movement;
        else
            cursorPosition += movement * (Screen.height / 2.0f);
        
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0.0f, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0.0f, Screen.height);

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(marker.panel, new Vector2(cursorPosition.x, Screen.height - cursorPosition.y));

        marker.style.left = panelPosition.x - 12.0f;
        marker.style.top = panelPosition.y - 12.0f;
    }

    private void OnDisable()
    {
        marker?.RemoveFromHierarchy();
        marker = null;
    }
}
