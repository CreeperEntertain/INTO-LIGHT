using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
        List<Texture2D> foundTextures = new();
        List<VectorImage> foundVectorImages = new();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Texture2D texture && !foundTextures.Contains(texture))
                    foundTextures.Add(texture);
                else if (asset is VectorImage vectorImage && !foundVectorImages.Contains(vectorImage))
                    foundVectorImages.Add(vectorImage);
        }
        foundTextures.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        foundVectorImages.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

        SerializedObject serialized = new(target);

        SerializedProperty textureArray = serialized.FindProperty("icons");
        textureArray.arraySize = foundTextures.Count;
        for (int i = 0; i < foundTextures.Count; i++)
            textureArray.GetArrayElementAtIndex(i).objectReferenceValue = foundTextures[i];

        SerializedProperty vectorArray = serialized.FindProperty("vectorIcons");
        vectorArray.arraySize = foundVectorImages.Count;
        for (int i = 0; i < foundVectorImages.Count; i++)
            vectorArray.GetArrayElementAtIndex(i).objectReferenceValue = foundVectorImages[i];
        
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
}
