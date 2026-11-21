
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

        public bool useComposite;
        public string up;
        public string down;
        public string left;
        public string right;
    }

    private sealed class RuntimeSource
    {
        public Source settings;
        public InputAction action;
    }

    private readonly List<RuntimeSource> sources = new();
    public bool IsRelative { get; private set; }

    public Vector2 Value {
        get
        {
            foreach (RuntimeSource source in sources)
            {
                Vector2 raw = source.action.ReadValue<Vector2>();
                if (raw == Vector2.zero)
                    continue;
                IsRelative = source.settings.mode == SourceMode.Relative;
                return source.settings.mode switch
                {
                    SourceMode.Position =>
                        Vector2.ClampMagnitude((raw - source.settings.center) * source.settings.scale, 1.0f),
                    SourceMode.Direction =>
                        Vector2.ClampMagnitude(raw, 1.0f),
                    _ => raw
                };
            }
            IsRelative = false;
            return Vector2.zero;
        }
    }


    public Vector2Input(string name, Source[] bindings)
    {
        if (bindings == null)
            return;
        foreach (Source source in bindings)
        {
            if (source == null)
                return;
            
            InputAction action = new($"{name} {sources.Count}", InputActionType.Value, expectedControlType: "Vector2");
            if (source.useComposite)
            {
                if (
                    string.IsNullOrWhiteSpace(source.up) ||
                    string.IsNullOrWhiteSpace(source.down) ||
                    string.IsNullOrWhiteSpace(source.left) ||
                    string.IsNullOrWhiteSpace(source.right)
                )
                {
                    action.Dispose();
                    continue;
                }
                action.AddCompositeBinding("2DVector")
                    .With("Up", source.up)
                    .With("Down", source.down)
                    .With("Left", source.left)
                    .With("Right", source.right);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(source.binding))
                {
                    action.Dispose();
                    continue;
                }
                action.AddBinding(source.binding);
            }
            sources.Add(new()
            {
                settings = source,
                action = action
            });
        }
    }


    public void Enable()
    {
        foreach (RuntimeSource source in sources)
            source.action.Enable();
    }

    public void Disable()
    {
        foreach (RuntimeSource source in sources)
            source.action.Disable();
    }

    public bool WasPressedThisFrame()
    {
        foreach (RuntimeSource source in sources)
            if (source.action.WasPressedThisFrame())
                return true;
        return false;
    }

    public bool IsPressed()
    {
        foreach (RuntimeSource source in sources)
            if (source.action.IsPressed())
                return true;
        return false;
    }

    public void Dispose()
    {
        foreach (RuntimeSource source in sources)
        {
            source.action.Disable();
            source.action.Dispose();
        }
        sources.Clear();
    }
}
