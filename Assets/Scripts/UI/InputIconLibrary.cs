using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InputIconLibrary", menuName = "UI/Input Icon Library")]
public sealed class InputIconLibrary : ScriptableObject
{
    public const string IconFolderPath = "Assets/Textures/UI/Icons/Input";

    [SerializeField] private Texture2D[] icons = Array.Empty<Texture2D>();

    private Dictionary<string, Texture2D> iconsByName;


    public Texture2D ResolveIcon(string requestedName, out string resolvedName)
    {
        EnsureLookup();
        if (!string.IsNullOrWhiteSpace(requestedName) && iconsByName.TryGetValue(requestedName, out Texture2D icon))
        {
            resolvedName = icon.name;
            return icon;
        }
        resolvedName = "undefined";
        return iconsByName.TryGetValue(resolvedName, out Texture2D fallback)
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
        if (icons == null)
            return;
        foreach (Texture2D icon in icons)
            if (icon != null)
                iconsByName[icon.name] = icon;
    }
}
