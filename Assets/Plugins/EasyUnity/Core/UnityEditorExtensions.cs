#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class UnityEditorExtensions
{
    public static void CreateDefaultScriptField<T>(UnityEngine.Object target) where T:UnityEngine.MonoBehaviour
    {
        GUI.enabled = false;
        MonoScript script = MonoScript.FromMonoBehaviour((T)target);
        EditorGUILayout.ObjectField("Script:", script , typeof(T), false);
        GUI.enabled = true;
        EditorGUILayout.Space();
    }
    /// <summary>
    /// Creates folders recursively, each within the previous one.
    /// </summary>
    /// <param name="parentFolder">Parent folder where action begins</param>
    /// <param name="newFolderNames">Names of folders to be created</param>
    /// <returns>GUID of created folders</returns>
    public static string[] CreateFolderRecursively(string parentFolder, params string[] newFolderNames)
    {
        _ = newFolderNames ?? throw new ArgumentNullException(nameof(newFolderNames));
        string[] guids = new string[newFolderNames.Length];
        string currentFolder = parentFolder;
        for (int i = 0; i < newFolderNames.Length; i++)
        {
            guids[i] = AssetDatabase.CreateFolder(currentFolder, newFolderNames[i]);
            currentFolder = AssetDatabase.GUIDToAssetPath(guids[i]);
        }
        return guids;
    }

    public static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        T t = AssetDatabase.LoadAssetAtPath<T>(path);
        if (!t)
        {
            t = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(t, path);
        }
        return t;
    }

    /// <summary>
    /// Gets a folder if it exists, creates it otherwise
    /// </summary>
    /// <param name="parentFolder">The name of the parent folder</param>
    /// <param name="newFolderName">The name of the new folder</param>
    /// <returns>The GUID of the desired folder</returns>
    public static string GetOrCreateFolder(string parentFolder, string newFolderName) => AssetDatabase.IsValidFolder($"{parentFolder}/{newFolderName}")
            ? AssetDatabase.AssetPathToGUID($"{parentFolder}/{newFolderName}")
            : AssetDatabase.CreateFolder(parentFolder, newFolderName);

    public static AnimationClip SetObjectReferenceCurve(this AnimationClip clip, EditorCurveBinding binding, ObjectReferenceKeyframe[] keyframes)
    {
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        return clip;
    }
}
#endif