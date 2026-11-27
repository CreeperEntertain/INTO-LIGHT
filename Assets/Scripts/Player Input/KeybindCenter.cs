using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "KeybindCenter", menuName = "Input/Keybind Center")]
public class KeybindCenter : ScriptableObject
{
    public const string PresetFolderPath = "Assets/ScriptableObjects/Input/Presets";
    public const string KeybindsAssetPath = "Assets/ScriptableObjects/Input/Keybinds.asset";

    [SerializeField] private PlayerInput[] presets = Array.Empty<PlayerInput>();

    private Dictionary<string, PlayerInput> presetsByName;

    public IReadOnlyList<PlayerInput> Presets
        => presets ?? Array.Empty<PlayerInput>();


    private void OnEnable()
        => presetsByName = null;

    private void OnValidate()
        => presetsByName = null;
    

    public PlayerInput GetPreset(string presetName)
        => TryGetPreset(presetName, out PlayerInput preset)
            ? preset : null;
    
    public bool TryGetPreset(string presetName, out PlayerInput preset)
    {
        EnsurePresetLookup();
        if (string.IsNullOrWhiteSpace(presetName))
        {
            preset = null;
            return false;
        }
        return presetsByName.TryGetValue(presetName.Trim(), out preset);
    }

    private void EnsurePresetLookup()
    {
        if (presetsByName != null)
            return;
        
        presetsByName = new(StringComparer.OrdinalIgnoreCase);
        if (presets == null)
            return;
        foreach (PlayerInput preset in presets)
        {
            if (preset == null || string.IsNullOrWhiteSpace(preset.name))
                continue;
            if (!presetsByName.ContainsKey(preset.name))
                presetsByName.Add(preset.name, preset);
        }
    }


#if UNITY_EDITOR

    public void RefreshPresets()
    {
        if (!AssetDatabase.IsValidFolder(PresetFolderPath))
        {
            Debug.LogError($"Preset folder not found: {PresetFolderPath}", this);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:PlayerInput", new[] { PresetFolderPath });
        List<PlayerInput> foundPresets = new();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PlayerInput preset = AssetDatabase.LoadAssetAtPath<PlayerInput>(path);
            if (preset != null && !foundPresets.Contains(preset))
                foundPresets.Add(preset);
        }
        foundPresets.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        presets = foundPresets.ToArray();
        presetsByName = null;

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public bool ReplaceKeybinds(PlayerInput preset)
    {
        if (preset == null)
        {
            Debug.LogError("Cannot apply a null keybind preset.", this);
            return false;
        }

        PlayerInput keybinds = AssetDatabase.LoadAssetAtPath<PlayerInput>(KeybindsAssetPath);
        if (keybinds == null)
        {
            Debug.LogError($"Keybinds asset not found: {KeybindsAssetPath}", this);
            return false;
        }
        if (preset == keybinds)
        {
            Debug.LogWarning("Cannot replace Keybinds with itself.", this);
            return false;
        }

        SerializedObject source = new(preset);
        SerializedObject destination = new(keybinds);
        source.Update();
        destination.Update();
        Undo.RecordObject(keybinds, $"Apply Keybind Preset: {preset.name}");
        SerializedProperty property = source.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.depth != 0)
                continue;
            
            string path = property.propertyPath;
            if (
                path.StartsWith("m_", StringComparison.Ordinal) ||
                path == "inputIconLibrary" ||
                path == "resetKeybinds"
            )
                continue;
            if (destination.FindProperty(path) != null)
                destination.CopyFromSerializedProperty(property);
        }

        destination.ApplyModifiedProperties();
        EditorUtility.SetDirty(keybinds);
        keybinds.RebuildActions();
        AssetDatabase.SaveAssets();
        Debug.Log($"Applied '{preset.name}' to Keybinds.", keybinds);
        return true;
    }

#endif
}
