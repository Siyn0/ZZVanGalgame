using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ZZVan.Galgame
{
    // Attach to a choice button. CanvasGroup keeps the evaluator alive while hiding the choice.
    public sealed class ConditionalAction : MonoBehaviour
    {
        public ConditionGroup condition = new ConditionGroup();
        public List<VariableChange> changes = new List<VariableChange>();
        public List<string> unlocks = new List<string>();
        public string destinationScene;
        public bool runOnStart;
        public UnityEvent onTriggered = new UnityEvent();
        private CanvasGroup visibility;
        private Button button;
        private bool triggered;
        public string actionId;
        private string Key => string.IsNullOrWhiteSpace(actionId) ? GameSession.ObjectId(transform) : actionId;
        private void Start()
        {
            button = GetComponent<Button>();
            if (button)
            {
                visibility = GetComponent<CanvasGroup>();
                if (!visibility) visibility = gameObject.AddComponent<CanvasGroup>();
                button.onClick.AddListener(Trigger);
            }
            GameSession.Instance.Changed += Refresh;
            Refresh();
            if (runOnStart) Trigger();
        }
        private void OnDestroy()
        {
            var session = FindObjectOfType<GameSession>();
            if (session) session.Changed -= Refresh;
            if (button) button.onClick.RemoveListener(Trigger);
        }
        public void Refresh()
        {
            if (!visibility) return;
            bool allowed = condition.Evaluate(GameSession.Instance.State);
            visibility.alpha = allowed ? 1 : 0;
            visibility.interactable = visibility.blocksRaycasts = allowed;
        }
        public void Trigger()
        {
            var session = GameSession.Instance;
            if (triggered || session.HasAction(Key) || !condition.Evaluate(session.State)) return;
            if (!string.IsNullOrEmpty(destinationScene) && !Application.CanStreamedLevelBeLoaded(destinationScene))
            { Debug.LogError("Scene is not in Build Settings: " + destinationScene); return; }
            triggered = true;
            session.MarkAction(Key);
            foreach (var change in changes) change.Apply(session.State);
            session.SyncLegacyState();
            foreach (var id in unlocks) session.Unlock(id);
            onTriggered.Invoke();
            if (!string.IsNullOrEmpty(destinationScene)) SceneManager.LoadScene(destinationScene);
        }
    }
}
