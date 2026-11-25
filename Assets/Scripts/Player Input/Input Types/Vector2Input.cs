
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
        Position,
        Tilt,
        Scroll
    }

    [Serializable]
    public class Source
    {
        public string binding;
        public SourceMode mode;
        public Vector2 center = new(0.5f, 0.5f);
        public float scale = 2.0f;
        public bool cullMotionBounds;
        [Min(0.0f)] public Vector2 motionSensitivity = new(1.0f, 1.0f);

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
        public Vector2 accumulatedAngleDegrees;
        public int lastMotionFrame = -1;
        public int motionStateVersion = -1;
    }

    private readonly List<RuntimeSource> sources = new();
    public bool IsRelative { get; private set; }

    public Vector2 Value {
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
                            source.accumulatedAngleDegrees = Vector2.zero;
                            source.motionStateVersion = JSLMotionInput.MotionStateVersion;
                        }
                        source.accumulatedAngleDegrees += Vector2.Scale(new(delta.x, delta.y), source.settings.motionSensitivity);
                        if (source.settings.cullMotionBounds)
                            source.accumulatedAngleDegrees = new(
                                Mathf.Clamp(source.accumulatedAngleDegrees.x, -180.0f, 180.0f),
                                Mathf.Clamp(source.accumulatedAngleDegrees.y, -180.0f, 180.0f)
                            );
                        else
                            source.accumulatedAngleDegrees = new(
                                Mathf.DeltaAngle(0.0f, source.accumulatedAngleDegrees.x),
                                Mathf.DeltaAngle(0.0f, source.accumulatedAngleDegrees.y)
                            );
                    }

                    HasInput = true;
                    ActiveMode = SourceMode.Tilt;
                    LastActiveMode = SourceMode.Tilt;
                    ActiveControl = null;
                    IsRelative = false;

                    return source.accumulatedAngleDegrees / 180.0f;
                }

                Vector2 raw = source.action.ReadValue<Vector2>();
                if (source.settings.mode == SourceMode.Scroll)
                {
                    float delta = -raw.y * source.settings.scale;
                    if (Mathf.Approximately(delta, 0.0f))
                        continue;
                    raw = new(delta, delta);
                }
                else if (raw == Vector2.zero)
                    continue;
                
                HasInput = true;
                ActiveMode = source.settings.mode;
                LastActiveMode = source.settings.mode;
                ActiveControl = source.action.activeControl;
                IsRelative = source.settings.mode is SourceMode.Relative or SourceMode.Scroll;
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

    public bool HasInput { get; private set; }
    public SourceMode? ActiveMode { get; private set; }
    public SourceMode LastActiveMode { get; private set; }
    public InputControl ActiveControl { get; private set; }


    public Vector2Input(string name, Source[] bindings)
    {
        if (bindings == null)
            return;
        foreach (Source source in bindings)
        {
            if (source == null)
                return;
            if (source.mode == SourceMode.Tilt)
            {
                sources.Add(new() { settings = source });
                continue;
            }
            
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
