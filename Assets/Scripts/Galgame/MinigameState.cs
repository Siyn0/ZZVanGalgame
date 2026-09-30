using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZVan.Galgame
{
    [Serializable]
    public sealed class MinigameState
    {
        public string id;
        public int score, mistakes, selected = -1, revealed = -1;
        public float revealRemaining;
        public float remaining = 30, x = 0.1f, y, velocity;
        public bool finished, won;
        public List<int> cards = new List<int>();
        public List<int> matched = new List<int>();
        public static bool Supports(string id) => id == "shooting" || id == "memory" || id == "platformer";
        public bool IsValid()
        {
            if (!Supports(id) || float.IsNaN(remaining) || float.IsInfinity(remaining) || remaining < 0 || remaining > 60 ||
                float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y) ||
                float.IsNaN(velocity) || float.IsInfinity(velocity) || x < 0 || x > 1 || y < 0 || score < 0) return false;
            if (id != "memory") return true;
            if (cards == null || matched == null || cards.Count != 8 || selected < -1 || selected >= 8 || revealed < -1 || revealed >= 8 ||
                float.IsNaN(revealRemaining) || float.IsInfinity(revealRemaining) || revealRemaining < 0 || revealRemaining > 1) return false;
            var counts = new int[4];
            foreach (int card in cards) { if (card < 0 || card > 3) return false; counts[card]++; }
            foreach (int count in counts) if (count != 2) return false;
            var unique = new HashSet<int>();
            foreach (int index in matched) if (index < 0 || index >= 8 || !unique.Add(index)) return false;
            return true;
        }
        public static MinigameState Create(string id)
        {
            if (!Supports(id)) throw new ArgumentException("Unknown minigame: " + id);
            var result = new MinigameState { id = id, remaining = id == "memory" ? 60 : 30 };
            if (id == "memory")
            {
                result.cards.AddRange(new[] { 0, 0, 1, 1, 2, 2, 3, 3 });
                for (int i = result.cards.Count - 1; i > 0; i--)
                { int j = UnityEngine.Random.Range(0, i + 1); int card = result.cards[i]; result.cards[i] = result.cards[j]; result.cards[j] = card; }
            }
            return result;
        }
        public void Tick(float delta)
        {
            if (finished) return;
            if (revealed >= 0)
            {
                revealRemaining = Mathf.Max(0, revealRemaining - delta);
                if (revealRemaining == 0) { selected = -1; revealed = -1; }
            }
            remaining = Mathf.Max(0, remaining - delta);
            if (remaining <= 0) { finished = true; won = false; }
        }
        public void Hit()
        {
            if (finished || id != "shooting") return;
            score++;
            if (score >= 10) { finished = won = true; }
        }
        public void Flip(int index)
        {
            if (finished || revealed >= 0 || id != "memory" || index < 0 || index >= cards.Count || matched.Contains(index) || selected == index) return;
            if (selected < 0) { selected = index; return; }
            if (cards[selected] == cards[index]) { matched.Add(selected); matched.Add(index); score++; }
            else mistakes++;
            revealed = index; revealRemaining = 0.7f;
            if (matched.Count == cards.Count) { finished = won = true; }
        }
        public void Move(float horizontal, bool jump, float delta)
        {
            if (finished || id != "platformer") return;
            if (jump && y <= 0) velocity = 1.8f;
            x = Mathf.Clamp01(x + horizontal * delta * 0.25f);
            velocity -= delta * 4;
            y = Mathf.Max(0, y + velocity * delta);
            if (y <= 0) velocity = 0;
            if ((x > 0.32f && x < 0.40f || x > 0.65f && x < 0.73f) && y < 0.1f)
            { finished = true; won = false; }
            if (x >= 0.96f) { finished = won = true; }
        }
    }
}
