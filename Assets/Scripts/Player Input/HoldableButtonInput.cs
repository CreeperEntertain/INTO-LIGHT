using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class HoldableButtonInput : IDisposable
{
    public string binding;
    [Min(0.0f)] public float pressTime;

    private sealed class RuntimeBinding
    {
        public InputAction action;
        public float pressedAt;
        public bool wasHeld;
        public bool pressed;
        public bool pressedThisFrame;
    }

    private readonly List<RuntimeBinding> bindings = new();
    private bool isRuntimeInput;
    private bool stateUpdatedThisFrame;
    private int lastUpdateFrame = -1;

    private readonly List<float> sourceTimes = new();

    private bool releasedBeforePressTime;
    public bool WasReleasedBeforePressTime
    {
        get
        {
            UpdateState();
            return releasedBeforePressTime;
        }
    }
    public event Action<float> ReleasedBeforePressTime;


    public HoldableButtonInput() {}

    public HoldableButtonInput(string name, HoldableButtonInput[] sources)
    {
        isRuntimeInput = true;
        if (sources == null)
            return;
        
        foreach (HoldableButtonInput source in sources)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.binding))
                continue;
            
            InputAction action = new($"{name} {bindings.Count}", InputActionType.Button);
            action.AddBinding(source.binding);
            bindings.Add(new RuntimeBinding
            {
                action = action,
                pressedAt = 0.0f,
                wasHeld = false,
                pressed = false,
                pressedThisFrame = false
            });
            sourceTimes.Add(source.pressTime);
        }
    }


    public bool IsPressed()
    {
        UpdateState();
        foreach (RuntimeBinding binding in bindings)
            if (binding.pressed)
                return true;
        return false;
    }

    public bool WasPressedThisFrame()
    {
        UpdateState();
        foreach (RuntimeBinding binding in bindings)
            if (binding.pressedThisFrame)
                return true;
        return false;
    }

    public void Enable()
    {
        foreach (RuntimeBinding binding in bindings)
            binding.action.Enable();
    }

    public void Disable()
    {
        foreach (RuntimeBinding binding in bindings)
            binding.action.Disable();
        ResetState();
    }

    private void UpdateState()
    {
        if (
            !isRuntimeInput ||
            (stateUpdatedThisFrame && lastUpdateFrame == Time.frameCount)
        )
            return;

        stateUpdatedThisFrame = true;
        lastUpdateFrame = Time.frameCount;
        releasedBeforePressTime = false;
        float currentTime = Time.unscaledTime;
        for (int i = 0; i < bindings.Count; i++)
        {
            RuntimeBinding binding = bindings[i];
            binding.pressedThisFrame = false;
            if (!binding.action.enabled)
            {
                binding.wasHeld = false;
                binding.pressed = false;
                continue;
            }
            if (!binding.action.IsPressed())
            {
                if (binding.wasHeld && !binding.pressed)
                {
                    float heldDuration = currentTime - binding.pressedAt;
                    if (heldDuration < sourceTimes[i])
                    {
                        releasedBeforePressTime = true;
                        ReleasedBeforePressTime?.Invoke(heldDuration);
                    }
                }
                binding.wasHeld = false;
                binding.pressed = false;
                continue;
            }
            if (!binding.wasHeld)
            {
                binding.wasHeld = true;
                binding.pressedAt = currentTime;
                binding.pressed = false;
            }
            if (!binding.pressed && currentTime - binding.pressedAt >= Mathf.Max(0.0f, sourceTimes[i]))
            {
                binding.pressed = true;
                binding.pressedThisFrame = true;
            }
        }
    }

    private void ResetState()
    {
        stateUpdatedThisFrame = false;
        lastUpdateFrame = -1;
        releasedBeforePressTime = false;
        foreach (RuntimeBinding binding in bindings)
        {
            binding.pressedAt = 0.0f;
            binding.wasHeld = false;
            binding.pressed = false;
            binding.pressedThisFrame = false;
        }
    }

    public void Dispose()
    {
        foreach (RuntimeBinding binding in bindings)
        {
            binding.action.Disable();
            binding.action.Dispose();
        }
        bindings.Clear();
        sourceTimes.Clear();
    }
}
