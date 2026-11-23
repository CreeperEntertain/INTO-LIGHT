using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vector3Input : IDisposable
{
    public enum SourceMode
    {
        Direction,
        Relative,
        Position,
        Tilt
    }

    [Serializable]
    public class Source
    {
        public string binding;
        public SourceMode mode;
        public Vector3 center = new(0.5f, 0.5f, 0.5f);
        public float scale = 2.0f;

        public bool cullMotionBounds;
        [Min(0.0f)] public Vector3 motionSensitivity = new(1.0f, 1.0f, 1.0f);

        public bool useComposite;
        public string up;
        public string down;
        public string left;
        public string right;
        public string forward;
        public string backward;
    }

    private sealed class RuntimeSource
    {
        public Source settings;
        public InputAction action;
        public Vector3 accumulatedAngleDegrees;
        public int lastMotionFrame = -1;
        public int motionStateVersion = -1;
    }

    private readonly List<RuntimeSource> sources = new();

    public bool IsRelative { get; private set; }

    public Vector3 Value
    {
        get
        {
            HasInput = false;
            ActiveMode = null;
            ActiveControl = null;
            foreach (RuntimeSource source in sources)
            {
                if (source.settings.mode == SourceMode.Tilt)
                {
                    if (!JSLMotionInput.TryGetGyroDelta(out Vector3 delta))
                        continue;
                    if (source.lastMotionFrame != Time.frameCount)
                    {
                        source.lastMotionFrame = Time.frameCount;
                        if (source.motionStateVersion != JSLMotionInput.MotionStateVersion)
                        {
                            source.accumulatedAngleDegrees = Vector3.zero;
                            source.motionStateVersion = JSLMotionInput.MotionStateVersion;
                        }

                        delta = Vector3.Scale(delta, source.settings.motionSensitivity);
                        source.accumulatedAngleDegrees += delta;
                        if (source.settings.cullMotionBounds)
                            source.accumulatedAngleDegrees = new(
                                Mathf.Clamp(source.accumulatedAngleDegrees.x, -180.0f, 180.0f),
                                Mathf.Clamp(source.accumulatedAngleDegrees.y, -180.0f, 180.0f),
                                Mathf.Clamp(source.accumulatedAngleDegrees.z, -180.0f, 180.0f)
                            );
                        else
                            source.accumulatedAngleDegrees = new(
                                Mathf.DeltaAngle(0.0f, source.accumulatedAngleDegrees.x),
                                Mathf.DeltaAngle(0.0f, source.accumulatedAngleDegrees.y),
                                Mathf.DeltaAngle(0.0f, source.accumulatedAngleDegrees.z)
                            );
                    }
                    
                    HasInput = true;
                    ActiveMode = SourceMode.Tilt;
                    LastActiveMode = SourceMode.Tilt;
                    ActiveControl = null;
                    IsRelative = false;

                    return source.accumulatedAngleDegrees / 180.0f;
                }

                object rawValue = source.action.ReadValueAsObject();
                Vector3 raw = rawValue switch
                {
                    Vector3 value => value,
                    Vector2 value => new(value.x, value.y, 0.0f),
                    _ => Vector3.zero
                };
                if (raw == Vector3.zero)
                    continue;
                
                HasInput = true;
                ActiveMode = source.settings.mode;
                LastActiveMode = source.settings.mode;
                ActiveControl = source.action.activeControl;
                IsRelative = source.settings.mode == SourceMode.Relative;
                return source.settings.mode switch
                {
                    SourceMode.Position =>
                        Vector3.ClampMagnitude((raw - source.settings.center) * source.settings.scale, 1.0f),
                    SourceMode.Direction =>
                        Vector3.ClampMagnitude(raw, 1.0f),
                    _ => raw
                };
            }
            IsRelative = false;
            return Vector3.zero;
        }
    }
    
    public bool HasInput { get; private set; }
    public SourceMode? ActiveMode { get; private set; }
    public SourceMode LastActiveMode { get; private set; }
    public InputControl ActiveControl { get; private set; }


    public Vector3Input(string name, Source[] bindings)
    {
        if (bindings == null)
            return;
        foreach (Source source in bindings)
        {
            if (source == null)
                continue;
            if (source.mode == SourceMode.Tilt)
            {
                sources.Add(new() { settings = source });
                continue;
            }
            
            InputAction action = new($"{name} {sources.Count}", InputActionType.Value);
            if (source.useComposite)
            {
                var composite = action.AddCompositeBinding("3DVector");
                bool hasBinding = false;
                if (!string.IsNullOrWhiteSpace(source.up))
                {
                    composite.With("Up", source.up);
                    hasBinding = true;
                }
                if (!string.IsNullOrWhiteSpace(source.down))
                {
                    composite.With("Down", source.down);
                    hasBinding = true;
                }
                if (!string.IsNullOrWhiteSpace(source.left))
                {
                    composite.With("Left", source.left);
                    hasBinding = true;
                }
                if (!string.IsNullOrWhiteSpace(source.right))
                {
                    composite.With("Right", source.right);
                    hasBinding = true;
                }
                if (!string.IsNullOrWhiteSpace(source.forward))
                {
                    composite.With("Forward", source.forward);
                    hasBinding = true;
                }
                if (!string.IsNullOrWhiteSpace(source.backward))
                {
                    composite.With("Backward", source.backward);
                    hasBinding = true;
                }

                if (!hasBinding)
                {
                    action.Dispose();
                    continue;
                }
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
            source.action?.Enable();
    }

    public void Disable()
    {
        foreach (RuntimeSource source in sources)
            source.action?.Disable();
    }

    public bool WasPressedThisFrame()
    {
        foreach (RuntimeSource source in sources)
            if (source.action != null && source.action.WasPressedThisFrame())
                return true;
        return false;
    }

    public bool IsPressed()
    {
        foreach (RuntimeSource source in sources)
            if (source.action != null && source.action.IsPressed())
                return true;
        return false;
    }

    public bool SharesActiveControlWith(Vector2Input other)
        =>  other != null &&
            ActiveControl != null &&
            ActiveControl == other.ActiveControl;
    public bool SharesActiveControlWith(Vector3Input other)
        =>  other != null &&
            ActiveControl != null &&
            ActiveControl == other.ActiveControl;
    
    public void Dispose()
    {
        foreach (RuntimeSource source in sources)
        {
            source.action?.Disable();
            source.action?.Dispose();
        }
        sources.Clear();
    }
}
