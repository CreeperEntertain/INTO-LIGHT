using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class AnimatorUpdateModeWarning
{
    private const string SessionKey = "AnimatorUpdateModeWarning.Dismissed";

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
        if (SessionState.GetBool(SessionKey, false))
            return;
        
        EditorGUILayout.HelpBox(
            "Make sure to only use \"Fixed\" in case you are animating a moving platform.",
            MessageType.Warning
        );
        if (GUILayout.Button("Dismiss"))
            SessionState.SetBool(SessionKey, true);
    }
}
