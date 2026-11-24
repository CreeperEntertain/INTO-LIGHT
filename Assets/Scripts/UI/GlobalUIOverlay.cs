using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GlobalUIOverlay : MonoBehaviour
{
    private VisualElement marker;


    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();

        document.panelSettings.sortingOrder = float.MaxValue;
        document.sortingOrder = float.MaxValue;

        VisualElement root = document.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;

        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);

        marker = new()
        {
            name = "OverlayMarker",
            pickingMode = PickingMode.Ignore
        };

        marker.style.position = Position.Absolute;
        marker.style.left = 0;
        marker.style.top = 0;

        marker.style.width = 24;
        marker.style.height = 24;

        marker.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.85f);

        marker.style.borderTopWidth = 5;
        marker.style.borderBottomWidth = 5;
        marker.style.borderLeftWidth = 5;
        marker.style.borderRightWidth = 5;

        marker.style.borderTopColor = Color.white;
        marker.style.borderBottomColor = Color.white;
        marker.style.borderLeftColor = Color.white;
        marker.style.borderRightColor = Color.white;

        marker.style.borderTopLeftRadius = 12;
        marker.style.borderTopRightRadius = 12;
        marker.style.borderBottomLeftRadius = 12;
        marker.style.borderBottomRightRadius = 12;

        root.Add(marker);
    }

    private void OnDisable()
        => marker?.RemoveFromHierarchy();
}
