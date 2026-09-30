using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZZVan.Galgame
{
    public interface ISceneCheckpoint
    {
        string CaptureCheckpoint();
    }

    [Serializable]
    public sealed class SceneCheckpoint
    {
        public string id, json;
    }

    // Saves the authored scenes without constructing or rearranging their UI.
    public sealed class SceneProgress : MonoBehaviour
    {
        private SaveData pending;
        private readonly Dictionary<string, string> checkpoints = new Dictionary<string, string>();
        public bool IsRestoring => pending != null;
        private void Awake() { SceneManager.sceneLoaded += OnLoaded; }
        private void OnDestroy() { SceneManager.sceneLoaded -= OnLoaded; }
        public static string Key(Component component)
        {
            var node = component.transform;
            string key = node.GetSiblingIndex().ToString();
            while (node.parent) { node = node.parent; key = node.GetSiblingIndex() + "/" + key; }
            return key + ":" + component.GetType().FullName;
        }
        public bool TryRestore<T>(Component component, out T value) where T : class
        {
            value = null;
            string key = Key(component);
            if (!checkpoints.TryGetValue(key, out var json)) return false;
            value = JsonUtility.FromJson<T>(json);
            checkpoints.Remove(key);
            return value != null;
        }
        public void Save(int slot)
        {
            var session = GameSession.Instance;
            var scene = SceneManager.GetActiveScene();
            if (scene.name == "0" || scene.name == "0SetName") throw new InvalidOperationException("开始剧情后才能保存。");
            if (IsRestoring) throw new InvalidOperationException("请等待读档完成。");
            var data = new SaveData { storyId = "scene", nodeId = scene.path, state = session.State,
                savedAt = DateTime.UtcNow.ToString("o"), history = new List<DialogueRecord>(session.History),
                music = GameAudio.Instance.Capture(), voice = GameAudio.Instance.CaptureVoice() };
            foreach (var root in scene.GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<MonoBehaviour>())
                    if (component is ISceneCheckpoint checkpoint)
                        data.sceneCheckpoints.Add(new SceneCheckpoint { id = Key(component), json = checkpoint.CaptureCheckpoint() });
            session.Saves.Save(slot, data);
        }
        public bool Load(SaveData data)
        {
            if (!Application.CanStreamedLevelBeLoaded(data.nodeId) || data.sceneCheckpoints == null) return false;
            var restored = new Dictionary<string, string>();
            foreach (var checkpoint in data.sceneCheckpoints)
            {
                if (checkpoint == null || string.IsNullOrEmpty(checkpoint.id) || string.IsNullOrEmpty(checkpoint.json) || restored.ContainsKey(checkpoint.id)) return false;
                restored.Add(checkpoint.id, checkpoint.json);
            }
            checkpoints.Clear();
            foreach (var entry in restored) checkpoints.Add(entry.Key, entry.Value);
            pending = data;
            GameSession.Instance.RestoreSceneState(data);
            Time.timeScale = 1;
            SceneManager.LoadScene(data.nodeId);
            return true;
        }
        private void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (pending != null && scene.path == pending.nodeId) StartCoroutine(FinishRestore());
            else { pending = null; checkpoints.Clear(); }
        }
        private IEnumerator FinishRestore()
        {
            // Let scene components restore themselves in Start before applying audio.
            yield return null;
            if (pending == null) yield break;
            GameAudio.Instance.StopGame();
            GameAudio.Instance.Restore(pending.music);
            GameAudio.Instance.RestoreVoice(pending.voice);
            pending = null;
        }
    }
}
