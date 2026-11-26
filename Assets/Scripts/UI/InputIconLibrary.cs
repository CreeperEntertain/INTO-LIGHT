using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "InputIconLibrary", menuName = "UI/Input Icon Library")]
public sealed class InputIconLibrary : ScriptableObject
{
    public const string IconFolderPath = "Assets/Textures/UI/Icons/Input";

    [SerializeField] private Texture2D[] icons = Array.Empty<Texture2D>();
    [SerializeField] private VectorImage[] vectorIcons = Array.Empty<VectorImage>();

    private Dictionary<string, UnityEngine.Object> iconsByName;


    public UnityEngine.Object ResolveIcon(string requestedName, out string resolvedName)
    {
        EnsureLookup();
        if (!string.IsNullOrWhiteSpace(requestedName) && iconsByName.TryGetValue(requestedName, out UnityEngine.Object icon))
        {
            resolvedName = icon.name;
            return icon;
        }
        if (string.Equals(requestedName, "empty", StringComparison.OrdinalIgnoreCase))
        {
            resolvedName = "empty";
            return null;
        }
        resolvedName = "undefined";
        return iconsByName.TryGetValue(resolvedName, out UnityEngine.Object fallback)
            ? fallback : null;
    }

    private void OnEnable()
        => iconsByName = null;

    private void OnValidate()
        => iconsByName = null;

    private void EnsureLookup()
    {
        if (iconsByName != null)
            return;
        iconsByName = new(StringComparer.OrdinalIgnoreCase);
        if (icons != null)
            foreach (Texture2D icon in icons)
                if (icon != null)
                    iconsByName[icon.name] = icon;
        if (vectorIcons != null)
            foreach (VectorImage icon in vectorIcons)
                if (icon != null)
                    iconsByName[icon.name] = icon;
    }
}
