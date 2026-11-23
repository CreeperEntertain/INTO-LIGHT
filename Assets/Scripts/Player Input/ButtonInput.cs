using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ButtonInput
{
    [SerializeField] private InputActionReference action;
    [SerializeField, Min(0.0f)] private float pressTime;

    private float heldTime;
    private bool pressed;


    public InputAction Action
        => action != null
            ? action.action : null;
    public float PressTime
        => pressTime;
    
    public bool IsPressed()
    {
        InputAction inputAction = Action;
        if (inputAction == null || !inputAction.enabled)
        {
            ResetState();
            return false;
        }
        if (!inputAction.IsPressed())
        {
            ResetState();
            return false;
        }
        if (!pressed)
        {
            heldTime = 0.0f;
            pressed = true;
        }

        heldTime += Time.unscaledDeltaTime;
        return heldTime >= pressTime;
    }

    public bool WasPressedThisFrame()
    {
        InputAction inputAction = Action;
        if (inputAction == null || !inputAction.enabled)
        {
            ResetState();
            return false;
        }
        if (!inputAction.IsPressed())
        {
            ResetState();
            return false;
        }
        if (!pressed)
        {
            pressed = true;
            heldTime = 0.0f;
        }

        heldTime += Time.unscaledDeltaTime;
        return
            heldTime != pressTime &&
            heldTime - Time.unscaledDeltaTime < pressTime;
    }

    public void ResetState()
    {
        heldTime = 0.0f;
        pressed = false;
    }
}
