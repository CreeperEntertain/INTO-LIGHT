using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class PopupManager
{
    private const int SortingOrder = 1000;

    private static readonly Dictionary<string, PopupBox> popups = new();

    private static PanelSettings panelSettings;
    private static StyleSheet popupStyleSheet;

    private static GameObject host;
    private static UIDocument document;
    private static VisualElement root;

    private static bool IsInitialized
        => host != null && document != null && root != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatistics()
    {
        popups.Clear();

        panelSettings = null;
        popupStyleSheet = null;

        host = null;
        document = null;
        root = null;
    }


    public static void Initialize(PanelSettings settings, StyleSheet styleSheet)
    {
        if (settings == null || styleSheet == null)
        {
            Debug.LogError("PopupManager requires PanelSettings and a popup stylesheet.");
            return;
        }
        if (IsInitialized)
            return;

        popups.Clear();
        panelSettings = settings;
        popupStyleSheet = styleSheet;
        CreateHost();
    }

    private static void CreateHost()
    {
        host = new("Popup UI");
        Object.DontDestroyOnLoad(host);

        document = host.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;
        document.sortingOrder = SortingOrder;

        root = document.rootVisualElement;
        root.styleSheets.Add(popupStyleSheet);
    }

    private static bool EnsureInitialized()
    {
        if (IsInitialized)
            return true;
        if (panelSettings == null || popupStyleSheet == null)
        {
            Debug.LogError("PopupManager has not been initialized. Check PopupManagerBootstrap on the Player prefab.");
            return false;
        }
        if (host != null)
            Object.Destroy(host);

        host = null;
        document = null;
        root = null;
        popups.Clear();

        CreateHost();
        return IsInitialized;
    }

    public static void Show(string id, string title, string description, params PopupButton[] buttons)
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
        else if (popup.IsOpen)
        {
            popup.BringToFront();
            return;
        }
        popup.Show(title, description, buttons);
    }
}
