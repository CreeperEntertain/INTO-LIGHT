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

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private PauseBackgroundBlur blurPass;
    private SideMenuManager sideMenuManager;
    private ControlsMenuStateManager controlsMenuStateManager;

    private MenuButton resumeButton;
    private MenuButton optionsButton; 
    private MenuButton exitGameButton;

    private bool isOpen;
    private float blendStart;
    private float blendTarget;
    private float transitionStart;

    private float previousTimeScale = 1.0f;
    private bool previousAudioPause;


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
        if (keybinds.Pause.WasPressedThisFrame())
        {
            if (sideMenuManager != null)
                sideMenuManager.Hide();
            SetOpen(!isOpen);
        }
        if (Mathf.Approximately(blurPass.Blend, blendTarget))
            return;
        
        float t = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
        blurPass.Blend = Mathf.Lerp(blendStart, blendTarget, t);
    }

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        sideMenuManager = GetComponent<SideMenuManager>();
        if (sideMenuManager != null)
            sideMenuManager.Hide();

        controlsMenuStateManager = GetComponent<ControlsMenuStateManager>();
        if (controlsMenuStateManager == null)
            Debug.LogError("ControlsMenuStateManager is missing.", this);
        else
            controlsMenuStateManager.Close();

        resumeButton = root.Q<MenuButton>("ResumeButton");
        optionsButton = root.Q<MenuButton>("OptionsButton");
        exitGameButton = root.Q<MenuButton>("ExitGameButton");

        resumeButton?.RegisterCallback<ClickEvent>(_ => Resume());
        optionsButton?.RegisterCallback<ClickEvent>(_ => OpenOptions());
        exitGameButton?.RegisterCallback<ClickEvent>(_ => ExitGame());
    }

    private void OnDisable()
    {
        if (resumeButton != null)
            resumeButton.clicked -= Resume;
        if (optionsButton != null)
            optionsButton.clicked -= OpenOptions;
        if (exitGameButton != null)
            exitGameButton.clicked -= ExitGame;
        
        resumeButton?.UnregisterCallback<ClickEvent>(_ => Resume());
        optionsButton?.UnregisterCallback<ClickEvent>(_ => OpenOptions());
        exitGameButton?.UnregisterCallback<ClickEvent>(_ => ExitGame());

        if (controlsMenuStateManager != null)
            controlsMenuStateManager.Close();
        if (!isOpen)
            return;
        
        GameState.SetPaused(false);
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        isOpen = false;
    }


    private void Resume()
    {
        if (sideMenuManager != null)
            sideMenuManager.Hide();
        SetOpen(false);
    }

    private void OpenOptions()
    {
        
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
        if (controlsMenuStateManager != null)
            controlsMenuStateManager.Close();

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
}
