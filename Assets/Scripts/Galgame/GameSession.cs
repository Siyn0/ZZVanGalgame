using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ZZVan.Galgame
{
    public sealed class GameSession : MonoBehaviour
    {
        private static GameSession instance;
        public static GameSession Instance => instance;
        public GameState State { get; private set; } = new GameState();
        private readonly List<DialogueRecord> history = new List<DialogueRecord>();
        public IReadOnlyList<DialogueRecord> History => history;
        public CollectionData Collection { get; private set; }
        public SaveRepository Saves { get; private set; }
        public StoryRunner Story { get; private set; }
        public MinigameState Minigame { get; private set; }
        public string LastError { get; private set; }
        public SceneProgress SceneProgress { get; private set; }
        private CollectionCatalog catalog;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!instance) new GameObject("Galgame").AddComponent<GameSession>();
        }
        private void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            // A separate directory is deliberately used for the new format.
            string root = Environment.GetEnvironmentVariable("ZZVAN_TEST_SAVE_DIRECTORY");
            Saves = new SaveRepository(string.IsNullOrEmpty(root) ? Path.Combine(Application.persistentDataPath, "StorySaves") : root);
            Collection = Saves.ReadCollection();
            catalog = Resources.Load<CollectionCatalog>("GalgameCollection");
            gameObject.AddComponent<GameAudio>();
            SceneProgress = gameObject.AddComponent<SceneProgress>();
        }
        private void OnDestroy() { if (instance == this) instance = null; }
        public void BeginSceneGame(string playerName)
        {
            Story = null; Minigame = null;
            State = new GameState { playerName = string.IsNullOrWhiteSpace(playerName) ? "主角" : playerName.Trim() };
            history.Clear(); LastError = "";
            Time.timeScale = 1;
        }
        public void RestoreSceneState(SaveData data)
        {
            Story = null; Minigame = null; State = data.state;
            history.Clear(); history.AddRange(data.history);
            while (history.Count > 50) history.RemoveAt(0);
        }
        public void StartStory(StoryGraph graph, string playerName)
        {
            if (!graph) throw new InvalidOperationException("剧情资源不存在。");
            var errors = graph.Validate();
            if (errors.Count != 0) throw new InvalidOperationException(string.Join("\n", errors));
            GameAudio.Instance.StopGame();
            State = new GameState { playerName = string.IsNullOrWhiteSpace(playerName) ? "主角" : playerName.Trim() };
            history.Clear(); Minigame = null; LastError = "";
            Story = new StoryRunner(graph, State);
            Story.Entered += OnEntered;
            Story.Enter(graph.startNode);
        }
        public void ReturnToTitle()
        {
            Story = null; Minigame = null;
            Time.timeScale = 1;
            GameAudio.Instance.StopGame();
        }
        public string SpeakerName(StoryNode node)
        {
            if (node.speaker == Speaker.Protagonist) return State.playerName;
            if (node.speaker == Speaker.Narrator) return "";
            if (!string.IsNullOrWhiteSpace(node.speakerName)) return node.speakerName;
            switch (node.speaker)
            {
                case Speaker.Sister: return "妹妹";
                case Speaker.Satori: return "古明地觉";
                case Speaker.ZZ: return "ZZ";
                default: return "";
            }
        }
        private void OnEntered(StoryNode node)
        {
            Minigame = node.kind == NodeKind.Minigame ? MinigameState.Create(node.minigameId) : null;
            Unlock("node:" + Story.Graph.storyId + ":" + node.id);
            foreach (string id in node.unlocks) Unlock(id);
            if (!string.IsNullOrEmpty(node.endingId)) Unlock(node.endingId);
            if (!string.IsNullOrEmpty(node.text))
            {
                AddHistory(SpeakerName(node), State.Format(node.text));
                Unlock("achievement.first_dialogue");
            }
            if (node.speaker == Speaker.Sister) Unlock("character.sister");
            if (node.speaker == Speaker.Satori) Unlock("character.satori");
            if (node.speaker == Speaker.ZZ) Unlock("character.zz");
            ObserveAsset(node.background); ObserveAsset(node.portrait);
            if (node.musicCommand == MusicCommand.Play) GameAudio.Instance.PlayMusic(node.music, node.loopMusic);
            if (node.musicCommand == MusicCommand.FadeOut) GameAudio.Instance.FadeOut(node.fadeSeconds);
            GameAudio.Instance.PlayVoice(node.speaker, node.voice);
        }
        public void AddHistory(string speaker, string text)
        {
            history.Add(new DialogueRecord(speaker, text));
            while (history.Count > 50) history.RemoveAt(0);
        }
        public void Unlock(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || Collection.unlocked.Contains(id)) return;
            Collection.unlocked.Add(id);
            try { Saves.SaveCollection(Collection); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Collection.unlocked.Remove(id);
                LastError = "收集记录写入失败：" + e.Message;
                Debug.LogWarning(LastError);
            }
        }
        public bool IsUnlocked(string id) => Collection.unlocked.Contains(id);
        public void ObserveAsset(UnityEngine.Object asset)
        {
            if (!asset || !catalog) return;
            foreach (var entry in catalog.entries)
                if (entry.image == asset || entry.music == asset) Unlock(entry.id);
        }
        public void Save(int slot)
        {
            if (Story?.Current == null) { SceneProgress.Save(slot); return; }
            Saves.Save(slot, new SaveData { storyId = Story.Graph.storyId, nodeId = Story.Current.id,
                state = State, minigame = Minigame, music = GameAudio.Instance.Capture(), voice = GameAudio.Instance.CaptureVoice(),
                savedAt = DateTime.UtcNow.ToString("o"), history = new List<DialogueRecord>(history) });
        }
        public bool Load(int slot)
        {
            var data = Saves.Read(slot);
            if (data == null) return false;
            if (data.storyId == "scene") return SceneProgress.Load(data);
            var graph = Array.Find(Resources.LoadAll<StoryGraph>(""), g => g.storyId == data.storyId);
            if (!graph || graph.Validate().Count > 0) return false;
            var node = graph.Find(data.nodeId);
            if (node == null || node.kind == NodeKind.Branch) return false;
            if (node.kind == NodeKind.Minigame && (data.minigame == null || data.minigame.id != node.minigameId || !data.minigame.IsValid())) return false;
            // Restore presentation and state only: never run node rewards or append a duplicate log.
            var restored = new StoryRunner(graph, data.state);
            restored.Restore(data.nodeId);
            State = data.state; Story = restored; Story.Entered += OnEntered;
            Minigame = node.kind == NodeKind.Minigame ? data.minigame : null;
            history.Clear(); history.AddRange(data.history);
            while (history.Count > 50) history.RemoveAt(0);
            GameAudio.Instance.StopGame();
            GameAudio.Instance.Restore(data.music);
            GameAudio.Instance.RestoreVoice(data.voice);
            Time.timeScale = 1;
            return true;
        }
    }
}
