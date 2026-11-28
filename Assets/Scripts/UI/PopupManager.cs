using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class PopupManager
{
    private const int SortingOrder = 1000;

    private static readonly Dictionary<string, PopupBox> popups = new();

    private static GameObject host;
    private static VisualElement root;


    public static bool EnsureInitialized()
    {
        if (root != null)
            return true;
        
        PanelSettings panelSettings = Resources.Load<PanelSettings>("UI Toolkit/PanelSettings");
        StyleSheet popupStyleSheet = Resources.Load<StyleSheet>("UI/Styles/Elements/PopupBox");
        if (panelSettings == null || popupStyleSheet == null)
        {
            Debug.LogError("PopupManager: PanelSettings or PopupBox stylesheet was not found in Resources.");
            return false;
        }

        host = new("Popup UI");
        Object.DontDestroyOnLoad(host);

        UIDocument document = host.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;
        document.sortingOrder = SortingOrder;

        root = document.rootVisualElement;
        root.styleSheets.Add(popupStyleSheet);

        return true;
    }

    public static void Show(
        string id,
        string title,
        string description,
        params PopupButton[] buttons
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("PopupManager.Show requires a unique popup ID.");
            return;
        }
        if (!EnsureInitialized())
            return;
        if (!popups.TryGetValue(id, out PopupBox popup))
        {
            popup = new();
            popups.Add(id, popup);
            root.Add(popup);
        }
        popup.Show(title, description, buttons);
    }

    public static void Hide(string id)
    {
        if (popups.TryGetValue(id, out PopupBox popup))
            popup.Hide();
    }
}
