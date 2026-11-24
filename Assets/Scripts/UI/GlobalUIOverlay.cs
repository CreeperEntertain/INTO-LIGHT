using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GlobalUIOverlay : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private VisualElement marker;
    private Vector2 cursorPosition;
    private IPanel activePanel;

    private readonly List<(IPanel panel, float sortingOrder)> targetPanels = new();


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
    
    private void Start()
    {
        UIDocument overlay = GetComponent<UIDocument>();
        HashSet<IPanel> seenPanels = new();
        foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
        {
            if (document == overlay)
                continue;
            
            IPanel panel = document.rootVisualElement.panel;
            if (panel == null || !seenPanels.Add(panel))
                continue;

            targetPanels.Add((panel, document.panelSettings.sortingOrder));
        }

        targetPanels.Sort((a, b) => b.sortingOrder.CompareTo(a.sortingOrder));
    }

    private void Update()
    {
        if (marker == null || marker.panel == null || playerInput?.MoveCursor == null)
            return;

        Vector2 movement = playerInput.MoveCursor.Value;
        if (playerInput.MoveCursor.ActiveMode == Vector2Input.SourceMode.Tilt)
            cursorPosition = new(
                (movement.x + 1.0f) * 0.5f * Screen.width,
                (movement.y + 1.0f) * 0.5f * Screen.height
            );
        else if (playerInput.MoveCursor.IsRelative)
            cursorPosition += movement;

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0.0f, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0.0f, Screen.height);

        Vector2 screenPosition = new(cursorPosition.x, Screen.height - cursorPosition.y);
        Vector2 markerPosition = RuntimePanelUtils.ScreenToPanel(marker.panel, screenPosition);

        marker.style.left = markerPosition.x - 12.0f;
        marker.style.top = markerPosition.y - 12.0f;

        IPanel targetPanel = FindTargetPanel(screenPosition, out Vector2 panelPosition);
        if (activePanel != targetPanel)
        {
            if (activePanel != null)
                SendPointerMove(activePanel, new(-1.0f, -1.0f));
            activePanel = targetPanel;
        }
        if (activePanel == null)
            return;
        
        SendPointerMove(activePanel, panelPosition);
        if (playerInput.MenuSelect?.WasPressedThisFrame() == true)
            SendPointerClick(activePanel, panelPosition);
    }

    private void OnDisable()
    {
        if (activePanel != null)
            SendPointerMove(activePanel, new(-1.0f, -1.0f));
        activePanel = null;
        marker?.RemoveFromHierarchy();
        marker = null;
    }


    private IPanel FindTargetPanel(Vector2 screenPosition, out Vector2 panelPosition)
    {
        foreach (var target in targetPanels)
        {
            Vector2 position = RuntimePanelUtils.ScreenToPanel(target.panel, screenPosition);
            if (target.panel.Pick(position) == null)
                continue;

            panelPosition = position;
            return target.panel;
        }

        panelPosition = default;
        return null;
    }

    private static void SendPointerMove(IPanel panel, Vector2 position)
    {
        Event mouseEvent = new()
        {
            type = EventType.MouseMove,
            mousePosition = position
        };
        using (PointerMoveEvent pointerEvent = PointerMoveEvent.GetPooled(mouseEvent))
            panel.visualTree.SendEvent(pointerEvent);
    }

    private static void SendPointerClick(IPanel panel, Vector2 position)
    {
        Event mouseDown = new()
        {
            type = EventType.MouseDown,
            mousePosition = position,
            button = 0,
            clickCount = 1
        };
        using (PointerDownEvent pointerEvent = PointerDownEvent.GetPooled(mouseDown))
            panel.visualTree.SendEvent(pointerEvent);
        
        Event mouseUp = new()
        {
            type = EventType.MouseUp,
            mousePosition = position,
            button = 0,
            clickCount = 1
        };
        using (PointerUpEvent pointerEvent = PointerUpEvent.GetPooled(mouseUp))
            panel.visualTree.SendEvent(pointerEvent);
    }
}
