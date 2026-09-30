using System;
using System.Collections.Generic;

namespace ZZVan.Galgame
{
    public enum GameVariable { San, SisterAffection, SatoriAffection, ZZAffection, SisterRoute, SatoriRoute, ZZRoute }
    public enum Comparison { Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual }

    [Serializable]
    public sealed class GameState
    {
        public string playerName = "主角";
        public int san, sisterAffection, satoriAffection, zzAffection;
        public bool sisterRoute, satoriRoute, zzRoute;
        public int Get(GameVariable variable)
        {
            switch (variable)
            {
                case GameVariable.San: return san;
                case GameVariable.SisterAffection: return sisterAffection;
                case GameVariable.SatoriAffection: return satoriAffection;
                case GameVariable.ZZAffection: return zzAffection;
                case GameVariable.SisterRoute: return sisterRoute ? 1 : 0;
                case GameVariable.SatoriRoute: return satoriRoute ? 1 : 0;
                case GameVariable.ZZRoute: return zzRoute ? 1 : 0;
                default: throw new ArgumentOutOfRangeException(nameof(variable));
            }
        }
        public void Set(GameVariable variable, int value)
        {
            switch (variable)
            {
                case GameVariable.San: san = value; break;
                case GameVariable.SisterAffection: sisterAffection = value; break;
                case GameVariable.SatoriAffection: satoriAffection = value; break;
                case GameVariable.ZZAffection: zzAffection = value; break;
                case GameVariable.SisterRoute: sisterRoute = value != 0; break;
                case GameVariable.SatoriRoute: satoriRoute = value != 0; break;
                case GameVariable.ZZRoute: zzRoute = value != 0; break;
                default: throw new ArgumentOutOfRangeException(nameof(variable));
            }
        }
        public string Format(string text) => (text ?? "").Replace("{playerName}", playerName);
    }

    [Serializable]
    public sealed class VariableCondition
    {
        public GameVariable variable;
        public Comparison comparison;
        public int value;
        public bool Evaluate(GameState state)
        {
            int actual = state.Get(variable);
            switch (comparison)
            {
                case Comparison.Equal: return actual == value;
                case Comparison.NotEqual: return actual != value;
                case Comparison.Greater: return actual > value;
                case Comparison.GreaterOrEqual: return actual >= value;
                case Comparison.Less: return actual < value;
                case Comparison.LessOrEqual: return actual <= value;
                default: return false;
            }
        }
    }

    [Serializable]
    public sealed class ConditionGroup
    {
        public bool any;
        public bool negate;
        public List<VariableCondition> conditions = new List<VariableCondition>();
        [UnityEngine.SerializeReference] public List<ConditionGroup> groups = new List<ConditionGroup>();
        public bool Evaluate(GameState state)
        {
            bool result = !any;
            if (conditions.Count + groups.Count == 0) result = true;
            foreach (var condition in conditions) result = any ? result | condition.Evaluate(state) : result & condition.Evaluate(state);
            foreach (var group in groups) result = any ? result | group.Evaluate(state) : result & group.Evaluate(state);
            return negate ? !result : result;
        }
    }

    [Serializable]
    public sealed class VariableChange
    {
        public GameVariable variable;
        public bool add = true;
        public int value;
        public void Apply(GameState state) => state.Set(variable, add ? state.Get(variable) + value : value);
    }

    [Serializable]
    public sealed class DialogueRecord
    {
        public string speaker, text;
        public DialogueRecord(string speaker, string text) { this.speaker = speaker; this.text = text; }
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 2;
        public string storyId, nodeId, savedAt;
        public MusicState music = new MusicState();
        public VoiceState voice = new VoiceState();
        public GameState state = new GameState();
        public MinigameState minigame;
        public List<DialogueRecord> history = new List<DialogueRecord>();
        public List<SceneCheckpoint> sceneCheckpoints = new List<SceneCheckpoint>();
    }

    [Serializable]
    public sealed class CollectionData
    {
        public int version = 1;
        public List<string> unlocked = new List<string>();
    }
}
