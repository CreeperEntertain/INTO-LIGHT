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
            SetOpen(!isOpen);
        if (Mathf.Approximately(blurPass.Blend, blendTarget))
            return;
        
        float t = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
        blurPass.Blend = Mathf.Lerp(blendStart, blendTarget, t);
    }

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // UI
        root.Q<InputBindingFoldout>("PauseBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("MoveCursorBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("MenuSelectBindingsFoldout").Input = keybinds;
        // General
        root.Q<InputBindingFoldout>("JumpBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("MoveBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("MoveCameraBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("HandModeBindingsFoldout").Input = keybinds;
        // Motion Controls
        root.Q<InputBindingFoldout>("ResetMotionCalibrationBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("MoveHandBindingsFoldout").Input = keybinds;
        // Hand Controls
        root.Q<InputBindingFoldout>("GrabBindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("Pose1BindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("Pose2BindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("Pose3BindingsFoldout").Input = keybinds;
        root.Q<InputBindingFoldout>("Pose4BindingsFoldout").Input = keybinds;

        MenuButton resumeButton = root.Q<MenuButton>("ResumeButton");
        MenuButton optionsButton = root.Q<MenuButton>("OptionsButton");
        MenuButton controlsButton = root.Q<MenuButton>("ControlsButton");
        MenuButton exitGameButton = root.Q<MenuButton>("ExitGameButton");

        resumeButton.clicked += Resume;
        optionsButton.clicked += OpenOptions;
        controlsButton.clicked += OpenControls;
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


    private void Resume()
        => SetOpen(false);

    private void OpenOptions()
    {
        
    }

    private void OpenControls()
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
