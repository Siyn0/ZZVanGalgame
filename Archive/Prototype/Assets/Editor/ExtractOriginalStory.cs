using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZZVan.Galgame;

// One-time content extraction; archived with the original scene scripts after export.
public static class ExtractOriginalStory
{
    public static void Run()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var nodes = new List<StoryNode>();
        
        
        var destinations = new Dictionary<string, string>();
        var oldScenes = EditorBuildSettings.scenes.Select(s => s.path).Where(p => !p.EndsWith("/0.unity") && !p.EndsWith("/0SetName.unity")).ToArray();
        foreach (string path in oldScenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            string sceneName = scene.name;
            var images = UnityEngine.Object.FindObjectsOfType<Image>();
            var background = images.Where(i => i.sprite && (i.name == "bgImage" || i.name == "BG" || i.name == "Background"))
                .Select(i => i.sprite).FirstOrDefault();
            if (!background) background = images.Where(i => i.sprite && AssetDatabase.GetAssetPath(i.sprite).StartsWith("Assets/img/") &&
                !AssetDatabase.GetAssetPath(i.sprite).Contains("/mei/") && !AssetDatabase.GetAssetPath(i.sprite).Contains("/UI/"))
                .OrderByDescending(i => i.rectTransform.rect.width * i.rectTransform.rect.height).Select(i => i.sprite).FirstOrDefault();
            var dialogue = UnityEngine.Object.FindObjectOfType<TextChange>();
            if (dialogue)
            {
                var texts = new[] { dialogue.Text1, dialogue.Text2, dialogue.Text3, dialogue.Text4, dialogue.Text5, dialogue.Text6, dialogue.Text7, dialogue.Text8, dialogue.Text9, dialogue.Text10 };
                var names = new[] { dialogue.CText1, dialogue.CText2, dialogue.CText3, dialogue.CText4, dialogue.CText5, dialogue.CText6, dialogue.CText7, dialogue.CText8, dialogue.CText9, dialogue.CText10 };
                var sprites = new[] { dialogue.CSprite1, dialogue.CSprite2, dialogue.CSprite3, dialogue.CSprite4, dialogue.CSprite5, dialogue.CSprite6, dialogue.CSprite7, dialogue.CSprite8, dialogue.CSprite9, dialogue.CSprite10 };
                var audio = new[] { dialogue.Audio1, dialogue.Audio2, dialogue.Audio3, dialogue.Audio4, dialogue.Audio5, dialogue.Audio6, dialogue.Audio7, dialogue.Audio8, dialogue.Audio9, dialogue.Audio10 };
                int last = Array.FindLastIndex(texts, t => !string.IsNullOrEmpty(t));
                if (!string.IsNullOrEmpty(dialogue.LastText)) { int boundary = Array.IndexOf(texts, dialogue.LastText); if (boundary >= 0) last = boundary; }
                destinations[sceneName] = sceneName + ".1";
                for (int i = 0; i <= last; i++)
                {
                    var node = new StoryNode { id = sceneName + "." + (i + 1), text = (texts[i] ?? "").Replace("/PlayerName", "{playerName}"),
                        speaker = ParseSpeaker(names[i]), speakerName = names[i], portrait = sprites[i], background = background, voice = audio[i],
                        next = i == last ? "scene:" + dialogue.SceneName : sceneName + "." + (i + 2) };
                    if (node.speaker == Speaker.Protagonist) node.speakerName = "";
                    nodes.Add(node);
                }
            }
            else
            {
                var node = new StoryNode { id = sceneName + ".choice", kind = NodeKind.Choice, background = background, text = "请选择：" };
                foreach (var button in UnityEngine.Object.FindObjectsOfType<Button>().OrderBy(b => b.transform.GetSiblingIndex()))
                {
                    var mei = button.GetComponent<AddMeiPoint>(); var san = button.GetComponent<AddSan>(); var change = button.GetComponent<SceneChange>();
                    string target = mei ? mei.SceneName : san ? san.SceneName : change ? change.SceneName : null;
                    if (string.IsNullOrEmpty(target) || target == "0") continue;
                    var text = button.GetComponentInChildren<Text>();
                    var choice = new StoryChoice { text = text ? text.text : button.name, target = "scene:" + target };
                    if (mei) choice.changes.Add(new VariableChange { variable = GameVariable.SisterAffection, value = mei.MeiAdd });
                    if (san) choice.changes.Add(new VariableChange { variable = GameVariable.San, value = san.SanAdd });
                    node.choices.Add(choice);
                }
                destinations[sceneName] = node.id;
                nodes.Add(node);
            }
        }
        nodes.Add(new StoryNode { id = "excerpt.end", kind = NodeKind.End, text = "当前项目收录的剧情到这里结束。" });
        foreach (var node in nodes)
        {
            node.next = Resolve(node.next, destinations);
            foreach (var choice in node.choices) choice.target = Resolve(choice.target, destinations);
        }
        nodes[0].musicCommand = MusicCommand.Play;
        nodes[0].music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/miao5.mp3");
        var graph = ScriptableObject.CreateInstance<StoryGraph>(); graph.storyId = "main"; graph.startNode = "1.1"; graph.nodes = nodes;
        if (graph.Validate().Count > 0) throw new Exception(string.Join("\n", graph.Validate()));
        graph.hideFlags = HideFlags.None;
        AssetDatabase.CreateAsset(graph, "Assets/Resources/MainStory.asset");
        GalgameSetup.RebuildAudioLibrary(); GalgameSetup.CreateCollection();
        var runtime = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        camera.gameObject.AddComponent<AudioListener>();
        EditorSceneManager.SaveScene(runtime, "Assets/Scenes/Runtime.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Runtime.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("STORY_EXTRACTION_PASSED: " + nodes.Count + " nodes");
        EditorApplication.Exit(0);
    }
    private static string Resolve(string id, Dictionary<string, string> map)
    {
        if (id == null || !id.StartsWith("scene:")) return id;
        return map.TryGetValue(id.Substring(6), out var target) ? target : "excerpt.end";
    }
    private static Speaker ParseSpeaker(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Speaker.Narrator;
        if (name == "/PlayerName") return Speaker.Protagonist;
        if (name == "纯乃" || name == "妹妹" || name == "妹") return Speaker.Sister;
        if (name == "古明地觉" || name == "觉") return Speaker.Satori;
        if (string.Equals(name, "ZZ", StringComparison.OrdinalIgnoreCase)) return Speaker.ZZ;
        return Speaker.Other;
    }
}
