using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SideMenuManager : MonoBehaviour
{
    [SerializeField] private string panelName = "SideMenuPanel";

    private UIDocument uiDocument;
    private VisualElement sideMenuPanel;


    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        ResolvePanel();
    }

    public VisualElement Show(VisualTreeAsset menu)
    {
        if (menu == null)
        {
            Debug.LogError("Cannot show a null UXML menu.", this);
            return null;
        }
        if (sideMenuPanel == null && !ResolvePanel())
            return null;
        
        sideMenuPanel.Clear();
        VisualElement instance = menu.CloneTree();
        sideMenuPanel.Add(instance);
        return instance;
    }

    public void Hide()
    {
        if (sideMenuPanel == null && !ResolvePanel())
            return;
        sideMenuPanel.Clear();
    }

    private bool ResolvePanel()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
            return false;
        sideMenuPanel = uiDocument.rootVisualElement.Q<VisualElement>(panelName);
        if (sideMenuPanel == null)
        {
            Debug.LogError($"Side menu panel '{panelName}' was not found in the UIDocument.", this);
            return false;
        }
        return true;
    }
}
