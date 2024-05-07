using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
public class SceneViewWindow : EditorWindow
{
    protected Vector2 scrollPosition;
    protected OpenSceneMode openSceneMode = OpenSceneMode.Single;
    [MenuItem("Tools/Scenes View #s")]
    public static void Init()
    {
        var window = EditorWindow.GetWindow<SceneViewWindow>("Scenes View");
        window.minSize = new Vector2(250f, 200f);
        window.Show();
    }
    protected virtual void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.EndHorizontal();
        this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
        EditorGUILayout.BeginVertical();
        ScenesTabGUI();
       
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        GUILayout.Label("Credits YJ and Google", EditorStyles.centeredGreyMiniLabel);
    }
    protected virtual void ScenesTabGUI()
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        if (guids.Length == 0)
        {
            GUILayout.Label("No Scenes Found", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("Create New Scenes", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("And Switch Between them here", EditorStyles.centeredGreyMiniLabel);
        }
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorBuildSettingsScene buildScene = buildScenes.Find((editorBuildScene) =>
            {
                return editorBuildScene.path == path;
            });
            Scene scene = SceneManager.GetSceneByPath(path);
            bool isOpen = scene.IsValid() && scene.isLoaded;
            EditorGUI.BeginDisabledGroup(isOpen);
         
            if (buildScene != null)
            {
                if (GUILayout.Button(sceneAsset.name))
                {
                    Open(path);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        if (GUILayout.Button("Create New Scene"))
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene);
        }
    }
    public virtual void Open(string path)
    {
        if (EditorSceneManager.EnsureUntitledSceneHasBeenSaved("You don't have saved the Untitled Scene, Do you want to leave?"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path, this.openSceneMode);
        }
    }
}