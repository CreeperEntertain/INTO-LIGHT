using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

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
        
        float t = Mathf.Clamp01((Time.unscaledTime - transitionStart) / transitionDuration);
        blurPass.Blend = Mathf.Lerp(blendStart, blendTarget, t);
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
