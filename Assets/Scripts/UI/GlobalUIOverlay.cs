using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GlobalUIOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Settings")]
    [SerializeField, Min(0.0f)] private float directionalCursorSpeed = 900.0f;
    [SerializeField, Min(0.0f)] private float directionalScrollSpeed = 600.0f;
    [SerializeField, Min(0.0f)] private float wheelScrollSpeed = 5000.0f;

    private const int VirtualPointerFingerId = 0;

    private readonly List<(IPanel panel, float sortingOrder)> targetPanels = new();

    private VisualElement marker;
    private Vector2 cursorPosition;
    private IPanel activePanel;

    public static bool Enabled { get; private set; } = false;


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
        marker.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.85f);

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

        Enabled = false;
        marker.style.display = DisplayStyle.None;
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
        if (!Enabled)
            return;

        Vector2 movement = playerInput.MoveCursor.Value;
        if (playerInput.MoveCursor.ActiveMode == Vector2Input.SourceMode.Tilt)
            cursorPosition = new(
                (movement.x + 1.0f) * 0.5f * Screen.width,
                (movement.y + 1.0f) * 0.5f * Screen.height
            );
        else if (playerInput.MoveCursor.ActiveMode == Vector2Input.SourceMode.Direction)
            cursorPosition += directionalCursorSpeed * Time.unscaledDeltaTime * movement;
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
        HandleScroll(activePanel, panelPosition);
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

    private static Touch CreateVirtualTouch(Vector2 position, TouchPhase phase)
        => new()
        {
            fingerId = VirtualPointerFingerId,
            position = position,
            rawPosition = position,
            phase = phase,
            deltaPosition = Vector2.zero,
            deltaTime = Time.unscaledDeltaTime,
            tapCount = 1,
            pressure = 1.0f,
            maximumPossiblePressure = 1.0f
        };

    private void HandleScroll(IPanel panel, Vector2 panelPosition)
    {
        if (playerInput.Scroll == null)
            return;
        
        Vector2 scroll = playerInput.Scroll.Value;
        if (scroll == Vector2.zero)
            return;
        
        VisualElement target = panel.Pick(panelPosition);
        while (target != null && target is not ScrollView)
            target = target.parent;
        if (target is not ScrollView scrollView)
            return;
        
        float amount = playerInput.Scroll.ActiveMode == Vector2Input.SourceMode.Direction
            ? directionalScrollSpeed * Time.unscaledDeltaTime : wheelScrollSpeed;
        scrollView.scrollOffset += scroll * amount;
    }

    private static void SendPointerMove(IPanel panel, Vector2 position)
    {
        if (!Enabled)
            return;

        Touch touch = CreateVirtualTouch(position, TouchPhase.Moved);
        using (PointerMoveEvent pointerEvent = PointerMoveEvent.GetPooled(touch))
            panel.visualTree.SendEvent(pointerEvent);
    }

    private static void SendPointerClick(IPanel panel, Vector2 position)
    {
        if (!Enabled)
            return;

        VisualElement target = panel.Pick(position);
        while (target != null && target is not Button)
            target = target.parent;
        if (target is not Button button)
            return;
        
        VisualElement previousFocus = panel.focusController.focusedElement as VisualElement;
        button.Focus();
        using (NavigationSubmitEvent submitEvent = NavigationSubmitEvent.GetPooled())
            button.SendEvent(submitEvent);
        if (previousFocus != null && previousFocus.panel == panel)
            previousFocus.Focus();
        else if (button.panel == panel)
            button.Blur();
    }


    public void Enable()
    {
        if (Enabled)
            return;
        cursorPosition = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Enabled = true;
        if (marker != null)
            marker.style.display = DisplayStyle.Flex;
    }

    public void Disable()
    {
        if (!Enabled)
            return;
        Enabled = false;
        if (marker != null)
            marker.style.display = DisplayStyle.None;
    }
}
