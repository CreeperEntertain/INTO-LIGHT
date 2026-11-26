using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputIconLibrary))]
public class InputIconLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (!GUILayout.Button("Refresh from Assets/Textures/UI/Icons/Input"))
            return;
        
        string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { InputIconLibrary.IconFolderPath });
        List<Texture2D> found = new();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Texture2D texture && !found.Contains(texture))
                    found.Add(texture);
        }
        found.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        SerializedObject serialized = new(target);
        SerializedProperty iconArray = serialized.FindProperty("icons");
        iconArray.arraySize = found.Count;
        for (int i = 0; i < found.Count; i++)
            iconArray.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
        
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
}
