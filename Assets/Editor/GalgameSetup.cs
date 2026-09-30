using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ZZVan.Galgame;

// Build a stable GUID -> audio reference table so saves can restore BGM from earlier scenes.
[InitializeOnLoad]
public sealed class GalgameSetup : IPreprocessBuildWithReport
{
    static GalgameSetup() { EditorApplication.delayCall += EnsureAudioLibrary; }
    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report) { RebuildAudioLibrary(); }
    private static void EnsureAudioLibrary()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists("Assets/Resources/GalgameAudioLibrary.asset")) RebuildAudioLibrary();
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists("Assets/Resources/GalgameCollection.asset")) CreateCollection();
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists("Assets/Resources/DemoStory.asset")) DemoStorySetup.Create();
    }
    [MenuItem("Galgame/Rebuild Audio Library")]
    public static void RebuildAudioLibrary()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        const string path = "Assets/Resources/GalgameAudioLibrary.asset";
        var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(path);
        if (!library) { library = ScriptableObject.CreateInstance<AudioLibrary>(); AssetDatabase.CreateAsset(library, path); }
        library.entries.Clear();
        foreach (string id in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" }))
            library.entries.Add(new AudioEntry { id = id, clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(id)) });
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Galgame/Create Starter Collection")]
    public static void CreateCollection()
    {
        const string path = "Assets/Resources/GalgameCollection.asset";
        if (File.Exists(path)) return;
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var catalog = ScriptableObject.CreateInstance<CollectionCatalog>();
        catalog.entries.Add(new CollectionEntry { id = "achievement.first_dialogue", title = "故事开始", description = "阅读第一句剧情。", kind = CollectionKind.Achievement });
        catalog.entries.Add(new CollectionEntry { id = "character.sister", title = "妹妹", description = "三位主要角色之一。", kind = CollectionKind.Character,
            image = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/mei/Mei_Normal.png") });
        catalog.entries.Add(new CollectionEntry { id = "character.satori", title = "古明地觉", description = "三位主要角色之一。", kind = CollectionKind.Character });
        catalog.entries.Add(new CollectionEntry { id = "character.zz", title = "ZZ", description = "三位主要角色之一。", kind = CollectionKind.Character });
        catalog.entries.Add(new CollectionEntry { id = "cg.eat", title = "用餐", kind = CollectionKind.CG,
            image = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/EatCG.jpg") });
        var music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/miao5.mp3");
        if (music) catalog.entries.Add(new CollectionEntry { id = "music.miao5", title = music.name, kind = CollectionKind.Music, music = music });
        AssetDatabase.CreateAsset(catalog, path);
        AssetDatabase.SaveAssets();
    }
}
