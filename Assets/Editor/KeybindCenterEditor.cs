using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KeybindCenter))]
public class KeybindCenterEditor : Editor
{
    private PlayerInput presetToApply;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        KeybindCenter center = (KeybindCenter)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh Presets"))
        {
            Undo.RecordObject(center, "Refresh Keybind Presets");
            center.RefreshPresets();
            serializedObject.Update();
        }

        EditorGUILayout.Space();
        presetToApply = (PlayerInput)EditorGUILayout.ObjectField(
            "Preset to Apply",
            presetToApply,
            typeof(PlayerInput),
            false
        );
        using (new EditorGUI.DisabledScope(presetToApply == null))
            if (GUILayout.Button("Replace Keybinds with Preset"))
                center.ReplaceKeybinds(presetToApply);
        
        EditorGUILayout.HelpBox(
            "Applying a preset preserves the target's Reset Keybinds and Input Icon Library settings.",
            MessageType.Info
        );
    }
}
