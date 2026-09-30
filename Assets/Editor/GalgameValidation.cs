using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZZVan.Galgame;

public static class GalgameValidation
{
    [MenuItem("Galgame/Run Core Validation")]
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "ZZVan-Tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            var state = new GameState { playerName = "测试", san = 20, sisterAffection = 8, zzRoute = true };
            Require(state.Format("{playerName}你好") == "测试你好", "Name substitution");
            Require(state.Get(GameVariable.ZZRoute) == 1, "Route state");
            var group = new ConditionGroup();
            group.conditions.Add(new VariableCondition { variable = GameVariable.San, comparison = Comparison.Greater, value = 20 });
            Require(!group.Evaluate(state), "Strict threshold");
            group.conditions[0].comparison = Comparison.GreaterOrEqual;
            Require(group.Evaluate(state), "Inclusive threshold");
            group.conditions.Add(new VariableCondition { variable = GameVariable.SisterAffection, comparison = Comparison.Greater, value = 10 });
            Require(!group.Evaluate(state), "AND"); group.any = true;
            Require(group.Evaluate(state), "OR"); group.negate = true;
            Require(!group.Evaluate(state), "NOT");
            var nested = new ConditionGroup(); nested.groups.Add(group);
            Require(!nested.Evaluate(state), "Nested group");
            var repository = new SaveRepository(directory);
            var save = new SaveData { storyId = "main", nodeId = "1.7", state = state };
            for (int slot = 1; slot <= SaveRepository.SlotCount; slot++) repository.Save(slot, save);
            Require(repository.Read(36).nodeId == "1.7", "36 slots and exact dialogue position");
            Require(repository.Read(1).state.zzRoute, "Route round trip");
            save.state.san = 35; repository.Save(1, save);
            Require(repository.Read(1).state.san == 35, "Overwrite");
            File.WriteAllText(Path.Combine(directory, "slot-01.json"), "invalid json");
            Require(repository.Read(1).state.san == 20, "Corrupt primary recovers backup");
            Require(repository.Read(2).state.san == 20, "Slot isolation");
            bool invalidRejected = false;
            try { repository.Read(0); } catch (ArgumentOutOfRangeException) { invalidRejected = true; }
            Require(invalidRejected, "Invalid slot rejected");
            var collection = new CollectionData(); collection.unlocked.Add("ending.true.sister");
            repository.SaveCollection(collection); repository.Read(2);
            Require(repository.ReadCollection().unlocked.Contains("ending.true.sister"), "Global collection isolation");
            Require(GameAudio.IsVoiced(Speaker.Sister) && GameAudio.IsVoiced(Speaker.Satori) && GameAudio.IsVoiced(Speaker.ZZ) && !GameAudio.IsVoiced(Speaker.Protagonist), "Voice whitelist");
            ValidateStory();
            ValidateMinigames();
            Debug.Log("GALGAME_VALIDATION_PASSED");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
        finally
        {
            // Only remove the uniquely named test directory created by this method.
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
    private static void Require(bool condition, string test)
    {
        if (!condition) throw new Exception("Validation failed: " + test);
    }
    private static void ValidateStory()
    {
        GalgameSetup.RebuildAudioLibrary(); DemoStorySetup.Create();
        foreach (var story in Resources.LoadAll<StoryGraph>("")) Require(story.Validate().Count == 0, story.name + " graph integrity");
        var graph = Resources.Load<StoryGraph>("DemoStory");
        var state = new GameState();
        var runner = new StoryRunner(graph, state);
        runner.Enter("choose");
        Require(!runner.Choose(4), "Hidden choice rejected below threshold");
        state.san = 2;
        Require(runner.Choose(4) && state.sisterRoute, "Hidden choice unlocks route");
        runner.Enter("ending.check");
        Require(runner.Current.id == "true", "Conditional true ending");
        state.sisterRoute = false; runner.Enter("ending.check");
        Require(runner.Current.id == "normal", "Conditional normal ending");
        state.san = 0; runner.Enter("ending.check");
        Require(runner.Current.id == "bad", "Conditional bad ending");
        runner.Enter("won"); int san = state.san;
        runner.Restore("won"); Require(state.san == san, "Restore does not replay rewards");
        runner.Enter("shooting"); runner.CompleteMinigame(true);
        Require(runner.Current.id == "won" && state.san == san + 1, "Minigame returns to narrative exactly once");
        runner.CompleteMinigame(true); Require(state.san == san + 1, "Duplicate minigame completion ignored");
        var main = Resources.Load<StoryGraph>("MainStory");
        Require(main.nodes.Count == 38, "Original 38 nodes retained");
        Require(main.Find("1.6").speaker == Speaker.Sister && main.Find("1.6").voice != null, "Original sister voice retained");
        var original = new StoryRunner(main, new GameState());
        original.Enter(main.startNode);
        int steps = 0;
        while (original.Current.kind == NodeKind.Dialogue && steps++ < 100) original.Advance();
        Require(original.Current.kind == NodeKind.Choice && original.Current.choices.Count == 3, "Original branching choices migrated");
    }
    private static void ValidateMinigames()
    {
        var shooting = MinigameState.Create("shooting");
        for (int i = 0; i < 10; i++) shooting.Hit();
        Require(shooting.finished && shooting.won, "Shooting victory");
        var timeout = MinigameState.Create("shooting"); timeout.Tick(31);
        Require(timeout.finished && !timeout.won, "Timeout failure");
        var memory = MinigameState.Create("memory");
        for (int value = 0; value < 4; value++)
        {
            for (int i = 0; i < memory.cards.Count; i++) if (memory.cards[i] == value) memory.Flip(i);
            memory.Tick(0.8f);
        }
        Require(memory.finished && memory.won && memory.IsValid(), "Memory pairing victory");
        var saved = JsonUtility.FromJson<MinigameState>(JsonUtility.ToJson(memory));
        Require(saved.IsValid() && saved.matched.Count == 8, "Minigame JSON state round trip");
        var platformer = MinigameState.Create("platformer");
        platformer.x = 0.34f; platformer.Move(0, false, 0.01f);
        Require(platformer.finished && !platformer.won, "Platform gap collision");
        platformer = MinigameState.Create("platformer"); platformer.x = 0.97f; platformer.Move(0, false, 0.01f);
        Require(platformer.won, "Platform finish");
    }
}
