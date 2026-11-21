
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Vector2Input : IDisposable
{
    public enum SourceMode
    {
        Direction,
        Relative,
        Position
    }

    [Serializable]
    public class Source
    {
        public string binding;
        public SourceMode mode;
        public Vector2 center = new(0.5f, 0.5f);
        public float scale = 2.0f;
    }

    private readonly InputAction action;
    private readonly List<Source> sources;
    private readonly Dictionary<string, Source> sourceSettings = new();
    private Vector2 value;

    public Vector2 Value => value;
    public bool IsRelative { get; private set; }

    public Vector2Input(string name, Source[] bindings)
    {
        sources = new List<Source>(bindings ?? Array.Empty<Source>());
        action = new InputAction(name, InputActionType.Value, expectedControlType: "Vector2");
        foreach (Source source in sources)
        {
            if (string.IsNullOrWhiteSpace(source.binding))
                continue;
            action.AddBinding(source.binding);
            sourceSettings[source.binding] = source;
        }

        action.performed += OnInput;
        action.canceled += OnInput;
    }

    public void Enable()
        => action.Enable();
    public void Disable()
        => action.Disable();

    public bool WasPressedThisFrame()
        => action.WasPressedThisFrame();
    public bool IsPressed()
        => action.IsPressed();

    public void Dispose()
    {
        action.performed -= OnInput;
        action.canceled -= OnInput;
        action.Dispose();
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            value = Vector2.zero;
            IsRelative = false;
            return;
        }

        string path = context.control.path;
        Source source = null;
        foreach (Source candidate in sources)
            if (
                candidate.binding == path ||
                context.control.path.EndsWith(candidate.binding.Replace("<Mouse>", "").Replace("<Gamepad>", ""))
            )
            {
                source = candidate;
                break;
            }
        if (source == null)
        {
            value = context.ReadValue<Vector2>();
            IsRelative = false;
            return;
        }

        Vector2 raw = context.ReadValue<Vector2>();
        IsRelative = source.mode == SourceMode.Relative;

        value = source.mode switch
        {
            SourceMode.Position => Vector2.ClampMagnitude((raw - source.center) * source.scale, 1.0f),
            SourceMode.Direction => Vector2.ClampMagnitude(raw, 1.0f),
            _ => raw
        };
    }
}
