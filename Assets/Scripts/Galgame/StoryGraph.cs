using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZVan.Galgame
{
    public enum Speaker { Narrator, Protagonist, Sister, Satori, ZZ, Other }
    public enum NodeKind { Dialogue, Choice, Branch, Minigame, End }
    public enum MusicCommand { Keep, Play, FadeOut }
    [Serializable]
    public sealed class StoryChoice
    {
        public string text, target;
        public ConditionGroup condition = new ConditionGroup();
        public List<VariableChange> changes = new List<VariableChange>();
    }
    [Serializable]
    public sealed class StoryNode
    {
        public string id;
        public NodeKind kind;
        public Speaker speaker;
        public string speakerName;
        [TextArea(2, 8)] public string text;
        public Sprite background, portrait;
        public AudioClip voice, music;
        public MusicCommand musicCommand;
        public bool loopMusic = true;
        public float fadeSeconds = 1;
        public string next, otherwise;
        public ConditionGroup condition = new ConditionGroup();
        public List<VariableChange> changes = new List<VariableChange>();
        public List<string> unlocks = new List<string>();
        public List<StoryChoice> choices = new List<StoryChoice>();
        public string minigameId, success, failure, endingId;
    }
    [CreateAssetMenu(menuName = "Galgame/Story Graph")]
    public sealed class StoryGraph : ScriptableObject
    {
        public string storyId = "main", startNode;
        public List<StoryNode> nodes = new List<StoryNode>();
        public StoryNode Find(string id) => nodes.Find(node => node.id == id);
        public List<string> Validate()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            foreach (var node in nodes)
                if (node == null || string.IsNullOrWhiteSpace(node.id) || !ids.Add(node.id)) errors.Add("Empty or duplicate node ID.");
            if (!ids.Contains(startNode)) errors.Add("Start node is missing: " + startNode);
            foreach (var node in nodes)
            {
                if (node == null) continue;
                foreach (string target in Targets(node))
                    if (string.IsNullOrEmpty(target) || !ids.Contains(target)) errors.Add(node.id + " -> missing target: " + target);
                if (node.kind == NodeKind.Choice && node.choices.Count == 0) errors.Add(node.id + ": choice node has no choices.");
                if (node.kind == NodeKind.Minigame && !MinigameState.Supports(node.minigameId)) errors.Add(node.id + ": unknown minigame: " + node.minigameId);
            }
            return errors;
        }
        public static IEnumerable<string> Targets(StoryNode node)
        {
            if (node.kind == NodeKind.Dialogue) yield return node.next;
            if (node.kind == NodeKind.Branch) { yield return node.next; yield return node.otherwise; }
            if (node.kind == NodeKind.Choice) foreach (var choice in node.choices) yield return choice.target;
            if (node.kind == NodeKind.Minigame) { yield return node.success; yield return node.failure; }
        }
    }
}
