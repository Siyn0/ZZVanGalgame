using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OriginalSceneValidation
{
    [MenuItem("Galgame/Validate Original Scene Bindings")]
    public static void Validate()
    {
        var active = SceneManager.GetActiveScene();
        if (!active.IsValid() || !active.isLoaded)
            active = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (var entry in EditorBuildSettings.scenes)
        {
            if (!entry.enabled) continue;
            var scene = SceneManager.GetSceneByPath(entry.path);
            bool wasLoaded = scene.IsValid() && scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var node in root.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0)
                            throw new Exception("Missing script in " + entry.path + ": " + node.name);
            }
            finally { if (!wasLoaded) EditorSceneManager.CloseScene(scene, true); }
        }
        if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        Debug.Log("ORIGINAL_SCENE_BINDINGS_PASSED");
    }
}
