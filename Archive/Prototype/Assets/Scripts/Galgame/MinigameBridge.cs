using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ZZVan.Galgame
{
    // A minigame owns its mechanics; this component owns its narrative outcome.
    public sealed class MinigameBridge : MonoBehaviour
    {
        public string successScene, failureScene;
        public List<VariableChange> successChanges = new List<VariableChange>();
        public List<VariableChange> failureChanges = new List<VariableChange>();
        public string successUnlock;
        public UnityEvent onSuccess = new UnityEvent(), onFailure = new UnityEvent();
        private bool finished;
        public void Succeed() => Finish(true);
        public void Fail() => Finish(false);
        private void Finish(bool success)
        {
            if (finished) return;
            string target = success ? successScene : failureScene;
            if (!string.IsNullOrEmpty(target) && !Application.CanStreamedLevelBeLoaded(target))
            { Debug.LogError("Minigame return scene missing: " + target); return; }
            finished = true;
            var session = GameSession.Instance;
            foreach (var change in success ? successChanges : failureChanges) change.Apply(session.State);
            session.SyncLegacyState();
            if (success) session.Unlock(successUnlock);
            (success ? onSuccess : onFailure).Invoke();
            if (!string.IsNullOrEmpty(target)) { Time.timeScale = 1; SceneManager.LoadScene(target); }
        }
    }
}
