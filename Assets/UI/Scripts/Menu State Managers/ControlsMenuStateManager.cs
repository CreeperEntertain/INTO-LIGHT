using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ControlsMenuStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput keybinds;
    [SerializeField] private KeybindCenter keybindCenter;
    [SerializeField] private VisualTreeAsset controlsMenu;
    [SerializeField] private VisualTreeAsset keybindPopupContent;
    [SerializeField] private VisualTreeAsset keybindPopupRow;

    private SideMenuManager sideMenuManager;
    private MenuButton controlsButton;

    private bool isOpen;


    private void Awake()
        => sideMenuManager = GetComponent<SideMenuManager>();
    
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        controlsButton = root.Q<MenuButton>("ControlsButton");
        if (controlsButton != null)
            controlsButton.clicked += OpenControls;
    }

    private void OnDisable()
    {
        if (controlsButton != null)
            controlsButton.clicked -= OpenControls;
        Close();
    }

    private void Update()
    {
        if (!isOpen || keybinds == null)
            return;
        if (keybinds.ResetKeybinds.WasPressedThisFrame())
            ShowResetKeybindsPopup();
    }


    private void OpenControls()
    {
        if (sideMenuManager == null)
        {
            Debug.LogError("SideMenuManager is missing.", this);
            return;
        }
        if (controlsMenu == null)
        {
            Debug.LogError("ControlsMenu UXML is not assigned.", this);
            return;
        }
        if (keybinds == null)
        {
            Debug.LogError("PlayerInput is not assigned.", this);
            return;
        }

        VisualElement controlsRoot = sideMenuManager.Show(controlsMenu);
        if (controlsRoot == null)
            return;
        
        controlsRoot.Query<InputBindingFoldout>().ForEach(foldout =>
        {
            foldout.PopupContentAsset = keybindPopupContent;
            foldout.PopupRowAsset = keybindPopupRow;
            foldout.Input = keybinds;
        });
        InputDisplayLabel resetLabel = controlsRoot.Q<InputDisplayLabel>("ResetKeybindsLabel");
        if (resetLabel != null)
            resetLabel.Input = keybinds;
        
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
        if (sideMenuManager != null)
            sideMenuManager.Hide();
    }


    private void ShowResetKeybindsPopup()
        => PopupManager.Show(
            "reset-controls",
            "Reset Controls",
            "Using this option will reset all your controls to factory defaults, overwriting what you have currently set. Alternatively, you can also do this by clicking \"Input Presets\" and selecting the \"Default\" option.",
            new PopupButton("Confirm", ResetKeybindsToDefault),
            new PopupButton("Cancel", () => {})
        );
    
    private void ResetKeybindsToDefault()
    {
        if (keybindCenter == null)
        {
            Debug.LogError("KeybindCenter is not assigned.", this);
            return;
        }

        PlayerInput defaultPreset = keybindCenter.GetPreset("Default");
        if (defaultPreset == null)
        {
            Debug.LogError("Default keybind preset was not found.", this);
            return;
        }
        if (!keybindCenter.ApplyPreset(keybinds, defaultPreset))
            Debug.LogError("Failed to reset keybinds to default.", this);
    }
}
