using UnityEditor;
using UnityEngine;
using ZZVan.Galgame;

public static class DemoStorySetup
{
    [MenuItem("Galgame/Create Demo Story")]
    public static void Create()
    {
        const string path = "Assets/Resources/DemoStory.asset";
        if (AssetDatabase.LoadAssetAtPath<StoryGraph>(path)) return;
        var graph = ScriptableObject.CreateInstance<StoryGraph>();
        graph.storyId = "demo"; graph.startNode = "intro";
        graph.nodes.Add(new StoryNode { id = "intro", speaker = Speaker.Sister, text = "{playerName}，来试试小游戏吧。每完成一个挑战，SAN 加 1。", next = "choose",
            portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/img/mei/Mei_Normal.png") });
        var choose = new StoryNode { id = "choose", kind = NodeKind.Choice, text = "选择挑战，或查看结局演示。" };
        choose.choices.Add(new StoryChoice { text = "移动靶射击", target = "shooting" });
        choose.choices.Add(new StoryChoice { text = "翻牌记忆", target = "memory" });
        choose.choices.Add(new StoryChoice { text = "跳跃挑战", target = "platformer" });
        choose.choices.Add(new StoryChoice { text = "查看当前结局", target = "ending.check" });
        var hidden = new StoryChoice { text = "隐藏选项：开启妹妹线（SAN ≥ 2）", target = "route" };
        hidden.condition.conditions.Add(new VariableCondition { variable = GameVariable.San, comparison = Comparison.GreaterOrEqual, value = 2 });
        choose.choices.Add(hidden); graph.nodes.Add(choose);
        foreach (string id in new[] { "shooting", "memory", "platformer" })
            graph.nodes.Add(new StoryNode { id = id, kind = NodeKind.Minigame, minigameId = id, success = "won", failure = "lost" });
        var won = new StoryNode { id = "won", text = "挑战成功！SAN +1。", next = "choose" };
        won.changes.Add(new VariableChange { variable = GameVariable.San, value = 1 });
        won.unlocks.Add("achievement.minigame"); graph.nodes.Add(won);
        graph.nodes.Add(new StoryNode { id = "lost", text = "挑战结束，再试一次吧。", next = "choose" });
        var route = new StoryNode { id = "route", speaker = Speaker.Sister, text = "隐藏剧情已触发，妹妹线已开启。", next = "choose" };
        route.changes.Add(new VariableChange { variable = GameVariable.SisterRoute, add = false, value = 1 });
        graph.nodes.Add(route);
        var ending = new StoryNode { id = "ending.check", kind = NodeKind.Branch, next = "true", otherwise = "normal.check" };
        ending.condition.conditions.Add(new VariableCondition { variable = GameVariable.SisterRoute, comparison = Comparison.Equal, value = 1 });
        graph.nodes.Add(ending);
        var normal = new StoryNode { id = "normal.check", kind = NodeKind.Branch, next = "normal", otherwise = "bad" };
        normal.condition.conditions.Add(new VariableCondition { variable = GameVariable.San, comparison = Comparison.Greater, value = 0 });
        graph.nodes.Add(normal);
        foreach (string id in new[] { "bad", "normal", "true" })
            graph.nodes.Add(new StoryNode { id = id, kind = NodeKind.End, text = "系统演示结束：" + id + " end。此处可换成正式结局剧情。", endingId = "demo.ending." + id });
        AssetDatabase.CreateAsset(graph, path);
        var catalog = AssetDatabase.LoadAssetAtPath<CollectionCatalog>("Assets/Resources/GalgameCollection.asset");
        if (catalog)
        {
            catalog.entries.Add(new CollectionEntry { id = "achievement.minigame", title = "挑战成功", description = "赢得一次小游戏。", kind = CollectionKind.Achievement });
            catalog.entries.Add(new CollectionEntry { id = "demo.ending.bad", title = "Bad End · 系统演示", kind = CollectionKind.BadEnd });
            catalog.entries.Add(new CollectionEntry { id = "demo.ending.normal", title = "Normal End · 系统演示", kind = CollectionKind.NormalEnd });
            catalog.entries.Add(new CollectionEntry { id = "demo.ending.true", title = "True End · 系统演示", kind = CollectionKind.TrueEnd });
            EditorUtility.SetDirty(catalog);
        }
        AssetDatabase.SaveAssets();
    }
}
