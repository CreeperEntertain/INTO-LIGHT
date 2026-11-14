using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class AnimatorUpdateModeWarning
{
    private const string WarningMessage = "Make sure to only use \"Fixed\" in case you are animating a moving platform.";

    static AnimatorUpdateModeWarning()
    {
        Editor.finishedDefaultHeaderGUI += DrawWarning;
    }

    private static void DrawWarning(Editor editor)
    {
        if (editor.target is not Animator animator)
            return;
        if (animator.updateMode == AnimatorUpdateMode.Fixed)
            return;

        string key = $"AnimatorUpdateModeWarning_Dismissed_{animator.GetInstanceID()}";
        if (SessionState.GetBool(key, false))
            return;
        
        EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
        if (GUILayout.Button("Dismiss"))
            SessionState.SetBool(key, true);
    }
}
