using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class ReleasableButtonInput : IDisposable
{
    public string binding;

    private sealed class RuntimeBinding{
        public InputAction action;
        public InputControl pressedControl;
        public float pressedAt;
        public bool pressed;
        public int releasedFrame = -1;
    }

    private readonly List<RuntimeBinding> bindings = new();
    private bool isRuntimeInput;
    private bool isEnabled;


    public ReleasableButtonInput() {}

    public ReleasableButtonInput(string name, ReleasableButtonInput[] sources)
    {
        isRuntimeInput = true;
        if (sources == null)
            return;

        foreach (ReleasableButtonInput source in sources)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.binding))
                continue;

            RuntimeBinding binding = new() { action = new($"{name} {bindings.Count}", InputActionType.Button) };
            
            binding.action.AddBinding(source.binding);
            binding.action.started += _ =>
            {
                binding.pressed = true;
                binding.pressedAt = Time.unscaledTime;
                binding.pressedControl = binding.action.activeControl;
            };
            binding.action.canceled += context =>
            {
                if (!isEnabled || !binding.pressed)
                {
                    binding.pressed = false;
                    return;
                }

                float heldDuration = Time.unscaledTime - binding.pressedAt;
                InputControl control = binding.pressedControl ?? context.control;
                float holdLimit = float.PositiveInfinity;
                bool hasHoldLimit =
                    control != null &&
                    ButtonHoldTimeRegistry.TryGetEarliestHoldTime(control, out holdLimit);
                if (!hasHoldLimit || heldDuration < holdLimit)
                    binding.releasedFrame = Time.frameCount;
                
                binding.pressed = false;
                binding.pressedControl = null;
            };

            bindings.Add(binding);
        }
    }


    public bool IsPressed()
    {
        foreach (RuntimeBinding binding in bindings)
            if (binding.pressed)
                return true;
        return false;
    }

    public bool WasPressedThisFrame()
    {
        foreach (RuntimeBinding binding in bindings)
            if (binding.releasedFrame == Time.frameCount)
                return true;
        return false;
    }

    public void Enable()
    {
        isEnabled = true;
        foreach (RuntimeBinding binding in bindings)
            binding.action.Enable();
    }

    public void Disable()
    {
        isEnabled = false;
        foreach (RuntimeBinding binding in bindings)
        {
            binding.action.Disable();
            binding.pressed = false;
            binding.pressedControl = null;
            binding.releasedFrame = -1;
        }
    }

    public void Dispose()
    {
        Disable();
        foreach (RuntimeBinding binding in bindings)
            binding.action.Dispose();
        bindings.Clear();
    }
}
