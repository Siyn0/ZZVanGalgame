using System;
using System.Collections.Generic;

namespace ZZVan.Galgame
{
    // No scene objects or UI dependencies: the graph owns narrative progression.
    public sealed class StoryRunner
    {
        public StoryGraph Graph { get; }
        public GameState State { get; }
        public StoryNode Current { get; private set; }
        public event Action<StoryNode> Entered;
        public StoryRunner(StoryGraph graph, GameState state)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }
        public void Restore(string id)
        {
            Current = Graph.Find(id) ?? throw new InvalidOperationException("Unknown story node: " + id);
            if (Current.kind == NodeKind.Branch) throw new InvalidOperationException("Cannot restore a transient branch.");
        }
        public void Enter(string id)
        {
            var seen = new HashSet<string>();
            while (true)
            {
                var node = Graph.Find(id) ?? throw new InvalidOperationException("Unknown story node: " + id);
                if (!seen.Add(id)) throw new InvalidOperationException("Automatic branch cycle: " + id);
                Current = node;
                foreach (var change in node.changes) change.Apply(State);
                Entered?.Invoke(node);
                if (node.kind == NodeKind.Branch)
                {
                    id = node.condition.Evaluate(State) ? node.next : node.otherwise;
                    continue;
                }
                return;
            }
        }
        public void Advance()
        {
            if (Current != null && Current.kind == NodeKind.Dialogue) Enter(Current.next);
        }
        public bool Choose(int index)
        {
            if (Current == null || Current.kind != NodeKind.Choice || index < 0 || index >= Current.choices.Count) return false;
            var choice = Current.choices[index];
            if (!choice.condition.Evaluate(State)) return false;
            if (Graph.Find(choice.target) == null) throw new InvalidOperationException("Choice target missing.");
            foreach (var change in choice.changes) change.Apply(State);
            Enter(choice.target);
            return true;
        }
        public void CompleteMinigame(bool success)
        {
            if (Current == null || Current.kind != NodeKind.Minigame) return;
            Enter(success ? Current.success : Current.failure);
        }
    }
}
