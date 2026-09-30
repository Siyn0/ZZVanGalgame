using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZZVan.Galgame;

public static class WorkbookSceneBuilder
{
    public const string Destination = "Assets/Scenes/WorkbookStory.unity";

    [MenuItem("Galgame/Prepare Workbook Story")]
    public static void Prepare()
    {
        DialogueWorkbookImporter.Import();
        // Never regenerate over a scene the artist may have edited.
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Destination)) CreateScene();
        var scenes = EditorBuildSettings.scenes.ToList();
        var existing = scenes.Find(s => s.path == Destination);
        if (existing != null) existing.enabled = true;
        else scenes.Add(new EditorBuildSettingsScene(Destination, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("WORKBOOK_STORY_PREPARED");
    }

    public static void PrepareBatch()
    {
        Prepare();
        OriginalSceneValidation.Validate();
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }

    private static void CreateScene()
    {
        if (!AssetDatabase.CopyAsset("Assets/Scenes/1.unity", Destination)) throw new InvalidOperationException("Cannot copy original dialogue scene.");
        var previous = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.OpenScene(Destination, OpenSceneMode.Additive);
        var source = SceneManager.GetSceneByPath("Assets/Scenes/3_Chose.unity");
        bool openedSource = !source.IsValid() || !source.isLoaded;
        if (openedSource) source = EditorSceneManager.OpenScene("Assets/Scenes/3_Chose.unity", OpenSceneMode.Additive);
        try
        {
            var next = Find<Button>(scene, "ChangeButton");
            var canvas = next.transform.parent;
            UnityEngine.Object.DestroyImmediate(next.GetComponent<TextChange>());
            var presenter = next.gameObject.AddComponent<WorkbookDialoguePresenter>();
            presenter.workbook = AssetDatabase.LoadAssetAtPath<DialogueWorkbook>(DialogueWorkbookImporter.Destination);
            presenter.sequence = presenter.workbook.rows[0].sequence;
            presenter.dialogueText = next.transform.Find("Text").GetComponent<Text>();
            presenter.characterName = next.transform.Find("CText").GetComponent<Text>();
            presenter.nameBackground = next.transform.Find("CharacterBG").GetComponent<Image>();
            presenter.background = Find<Image>(scene, "bgImage");
            presenter.background.raycastTarget = false;
            presenter.nextButton = next;
            next.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(next.onClick, presenter.Next);

            var girl = UnityEngine.Object.Instantiate(Find<Image>(source, "Girl").gameObject, canvas, false);
            girl.name = "Girl";
            girl.transform.SetSiblingIndex(next.transform.GetSiblingIndex());
            presenter.portrait = girl.GetComponent<Image>();
            presenter.portrait.sprite = null;
            presenter.portrait.enabled = false;
            presenter.portrait.raycastTarget = false;

            presenter.choiceButtons = new Button[4];
            string[] templates = { "A", "A", "B", "C" };
            for (int i = 0; i < templates.Length; i++)
            {
                var copy = UnityEngine.Object.Instantiate(Find<Button>(source, templates[i]).gameObject, canvas, false);
                copy.name = i == 0 ? "ExtraChoice" : templates[i];
                foreach (var behaviour in copy.GetComponents<MonoBehaviour>())
                    if (behaviour is SceneChange || behaviour is AddSan || behaviour is AddMeiPoint)
                        UnityEngine.Object.DestroyImmediate(behaviour);
                var button = copy.GetComponent<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                if (i == 0)
                {
                    // Extend the existing spacing upward so the original A/B/C and dialogue box stay put.
                    var rect = (RectTransform)copy.transform;
                    var first = (RectTransform)Find<Button>(source, "A").transform;
                    var second = (RectTransform)Find<Button>(source, "B").transform;
                    rect.anchoredPosition = new Vector2(first.anchoredPosition.x, first.anchoredPosition.y * 2 - second.anchoredPosition.y);
                }
                button.GetComponentInChildren<Text>(true).text = "";
                copy.SetActive(false);
                presenter.choiceButtons[i] = button;
            }
            // Original stage assets are retained; background placeholders are cleared by the presenter.
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Cannot save workbook scene.");
        }
        finally
        {
            if (openedSource) EditorSceneManager.CloseScene(source, true);
            EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
    }

    private static T Find<T>(Scene scene, string name) where T : Component => scene.GetRootGameObjects()
        .SelectMany(root => root.GetComponentsInChildren<T>(true)).Single(c => c.name == name);
}
