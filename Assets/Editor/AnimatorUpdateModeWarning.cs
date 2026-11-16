using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Animator))]
[CanEditMultipleObjects]
public class AnimatorUpdateModeWarning : Editor
{
    private const string DismissKey = "INTO_LIGHT.AnimatorUpdateModeWarning.Dismissed";
    private const string WarningText = "Make sure to only use \"Normal\" in case you are animating a moving platform.";

    private Editor builtInInspector;

    private void OnEnable()
    {
        Type inspectorType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType("UnityEditor.AnimatorInspector", false))
            .FirstOrDefault(type => type != null);
        if (inspectorType != null)
            builtInInspector = Editor.CreateEditor(targets, inspectorType);
    }

    public override void OnInspectorGUI()
    {
        if (builtInInspector == null)
        {
            EditorGUILayout.HelpBox(
                "Could not load Unity's built-in AnimatorInspector.",
                MessageType.Error);
            return;
        }

        builtInInspector.OnInspectorGUI();
        if (SessionState.GetBool(DismissKey, false))
            return;

        bool needsWarning = targets
            .OfType<Animator>()
            .Any(animator => animator.updateMode != AnimatorUpdateMode.Normal);
        if (!needsWarning)
            return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(WarningText, EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(20)))
            SessionState.SetBool(DismissKey, true);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void OnDisable()
    {
        if (builtInInspector != null)
            DestroyImmediate(builtInInspector);
    }
}