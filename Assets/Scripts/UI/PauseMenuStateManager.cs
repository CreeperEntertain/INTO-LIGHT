using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CustomPassVolume blurVolume;
    [SerializeField] private UIDocument uiDocument;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private PauseBackgroundBlur blurPass;
    private bool isOpen;
    private float blendStart;
    private float blendTarget;
    private float transitionStart;


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
    }

    private void Update()
    {
        if (playerInput.Pause.WasPressedThisFrame())
            SetOpen(!isOpen);
        if (Mathf.Approximately(blurPass.Blend, blendTarget))
            return;
        
        float t = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
        blurPass.Blend = Mathf.Lerp(blendStart, blendTarget, t);
    }

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        MenuButton resumeButton = root.Q<MenuButton>("ResumeButton");
        MenuButton optionsButton = root.Q<MenuButton>("OptionsButton");
        MenuButton controlsButton = root.Q<MenuButton>("ControlsButton");
        MenuButton exitGameButton = root.Q<MenuButton>("ExitGameButton");

        resumeButton.clicked += Resume;
        optionsButton.clicked += OpenOptions;
        controlsButton.clicked += OpenControls;
        exitGameButton.clicked += ExitGame;
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
        
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        blendStart = blurPass.Blend;
        blendTarget = open ? 1.0f : 0.0f;
        transitionStart = Time.unscaledTime;

        uiDocument.rootVisualElement.style.display =
            open ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
