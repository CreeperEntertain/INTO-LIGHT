using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PopupManager : MonoBehaviour
{
    private static PopupManager instance;

    [Header("Popup UI")]
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private StyleSheet popupStyleSheet;

    private UIDocument popupDocument;
    private PopupBox popup;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        GameObject popupObject = new("Popup UI");
        popupObject.transform.SetParent(transform, false);

        popupDocument = popupDocument.AddComponent<UIDocument>();
        popupDocument.panelSettings = panelSettings;
        popupDocument.sortingOrder = 1000;

        VisualElement root = popupDocument.rootVisualElement;
        root.styleSheets.Add(popupStyleSheet);

        popup = new();
        root.Add(popup);
    }

    public static void Show(string title, string description, params PopupButton[] buttons)
    {
        if (instance == null || instance.popup == null)
        {
            Debug.LogError("PopupManager is missing from the scene or has not initialized.");
            return;
        }
        instance.popup.Show(title, description, buttons);
    }

    public static void Hide()
    {
        if (instance != null && instance.popup != null)
            instance.popup.Hide();
    }
}
