using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput keybinds;
    [SerializeField] private CustomPassVolume blurVolume;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private KeybindCenter keybindCenter;
    [SerializeField] private VisualTreeAsset controlsMenu;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private PauseBackgroundBlur blurPass;
    private SideMenuManager sideMenuManager;

    private bool isOpen;
    private float blendStart;
    private float blendTarget;
    private float transitionStart;

    private float previousTimeScale = 1.0f;
    private bool previousAudioPause;


    private PopupBox resetKeybindsPopup;


    private void Awake()
    {
        foreach (var pass in blurVolume.customPasses)
            if (pass is PauseBackgroundBlur blur)
            {
                blurPass = blur;
                break;
            }
        if (blurPass == null)
        {
            Debug.LogError("PauseBackgroundBlur not found.", this);
            enabled = false;
            return;
        }

        blendStart = blurPass.Blend;
        blendTarget = 0.0f;
        transitionStart = Time.unscaledTime;
        isOpen = false;

        uiDocument.rootVisualElement.style.display = DisplayStyle.None;

        GameState.SetPaused(false);
    }

    private void Update()
    {
        if (isOpen && keybinds.ResetKeybinds.WasPressedThisFrame())
            PopupManager.Show(
                "reset-controls",
                "Reset Controls",
                "Using this option will reset all your controls to factory defaults, overwriting what you have currently set. Alternatively, you can also do this by clicking \"Input Presets\" and selecting the \"Default\" option.",
                new PopupButton("Confirm", ResetKeybindsToDefault),
                new PopupButton("Cancel", () => {})
            );
        if (keybinds.Pause.WasPressedThisFrame())
            SetOpen(!isOpen);
        if (Mathf.Approximately(blurPass.Blend, blendTarget))
            return;
        
        float t = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
        blurPass.Blend = Mathf.Lerp(blendStart, blendTarget, t);
    }

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        sideMenuManager = GetComponent<SideMenuManager>();
        sideMenuManager?.Hide();

        MenuButton resumeButton = root.Q<MenuButton>("ResumeButton");
        MenuButton optionsButton = root.Q<MenuButton>("OptionsButton");
        MenuButton controlsButton = root.Q<MenuButton>("ControlsButton");
        MenuButton exitGameButton = root.Q<MenuButton>("ExitGameButton");

        if (resumeButton != null)
            resumeButton.clicked += Resume;
        if (optionsButton != null)
            optionsButton.clicked += OpenOptions;
        if (controlsButton != null)
            controlsButton.clicked += OpenControls;
        if (exitGameButton != null)
            exitGameButton.clicked += ExitGame;
    }

    private void OnDisable()
    {
        if (!isOpen)
            return;
        
        GameState.SetPaused(false);
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        isOpen = false;
    }


    private VisualElement LoadControlsMenu()
    {
        if (sideMenuManager == null)
        {
            Debug.LogError("SideMenuManager is missing.", this);
            return null;
        }
        if (controlsMenu == null)
        {
            Debug.LogError("ControlsMenu UXML is not assigned.", this);
            return null;
        }
        return sideMenuManager.Show(controlsMenu);
    }


    private void Resume()
        => SetOpen(false);

    private void OpenOptions()
    {
        
    }

    private void OpenControls()
    {
        VisualElement controlsRoot = LoadControlsMenu();
        if (controlsRoot == null)
            return;
        controlsRoot.Query<InputBindingFoldout>().ForEach(foldout => foldout.Input = keybinds);
        InputDisplayLabel resetLabel = controlsRoot.Q<InputDisplayLabel>("ResetKeybindsLabel");
        if (resetLabel != null)
            resetLabel.Input = keybinds;
    }

    private void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }

    private void SetOpen(bool open)
    {
        if (isOpen == open)
            return;

        GameState.SetPaused(open);
        isOpen = open;
        if (open)
        {
            previousTimeScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;

            Time.timeScale = 0.0f;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioPause;
        }

        blendStart = blurPass.Blend;
        blendTarget = open ? 1.0f : 0.0f;
        transitionStart = Time.unscaledTime;

        uiDocument.rootVisualElement.style.display = open
            ? DisplayStyle.Flex : DisplayStyle.None;
    }


    private void ResetKeybindsToDefault()
    {
        if (keybindCenter == null)
        {
            Debug.LogError("Default keydind preset was not found.", this);
            return;
        }

        PlayerInput defaultPreset = keybindCenter.GetPreset("Default");
        if (defaultPreset == null)
        {
            Debug.LogError("Default keybind preset was not found.", this);
            return;
        }
        if (!keybindCenter.ApplyPreset(keybinds, defaultPreset))
            Debug.LogError("Failed to reset keybinds to defaults.", this);
    }
}
