using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GlobalUIOverlay : MonoBehaviour
{
    private VisualElement marker;


    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();

        document.panelSettings.sortingOrder = 10000f;
        document.sortingOrder = 10000f;

        VisualElement root = document.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;

        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);

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

    private void OnDisable()
    {
        marker?.RemoveFromHierarchy();
        marker = null;
    }
}
