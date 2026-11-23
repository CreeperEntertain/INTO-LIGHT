using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ButtonInput
{
    [SerializeField] private InputActionReference action;
    [SerializeField, Min(0.0f)] private float pressTime;

    [System.NonSerialized] private InputAction runtimeAction;
    [System.NonSerialized] private bool usesRuntimeAction;
    [System.NonSerialized] private bool stateUpdatedThisFrame;
    [System.NonSerialized] private int lastUpdateFrame;
    [System.NonSerialized] private float pressedAt;
    [System.NonSerialized] private bool wasHeld;
    [System.NonSerialized] private bool pressed;
    [System.NonSerialized] private bool pressedThisFrame;


    public InputAction Action
        => usesRuntimeAction
            ? runtimeAction : action != null
                ? action.action : null;
    public float PressTime
        => pressTime;

    public void SetAction(InputAction inputAction)
    {
        runtimeAction = inputAction;
        usesRuntimeAction = true;
        ResetState();
    }
    
    public bool IsPressed()
    {
        UpdateState();
        return pressed;
    }

    public bool WasPressedThisFrame()
    {
        UpdateState();
        return pressedThisFrame;
    }

    private void UpdateState()
    {
        if (stateUpdatedThisFrame && lastUpdateFrame == Time.frameCount)
            return;
        
        stateUpdatedThisFrame = true;
        lastUpdateFrame = Time.frameCount;
        pressedThisFrame = false;

        InputAction inputAction = Action;
        if (inputAction == null || !inputAction.enabled || !inputAction.IsPressed())
        {
            wasHeld = false;
            pressed = false;
            return;
        }

        float currentTime = Time.unscaledTime;
        if (!wasHeld)
        {
            wasHeld = true;
            pressedAt = currentTime;
            pressed = false;
        }
        if (!pressed && currentTime - pressedAt >= Mathf.Max(0.0f, pressTime))
        {
            pressed = true;
            pressedThisFrame = true;
        }
    }

    public void ResetState()
    {
        stateUpdatedThisFrame = false;
        lastUpdateFrame = -1;
        pressedAt = 0.0f;
        wasHeld = false;
        pressed = false;
        pressedThisFrame = false;
    }
}
