using UnityEditor;
using UnityEngine;
using ZZVan.Galgame;

public sealed class StoryGraphWindow : EditorWindow
{
    private StoryGraph graph;
    private Vector2 scroll;
    private int selected = -1;
    [MenuItem("Galgame/Story Graph")]
    public static void Open() { GetWindow<StoryGraphWindow>("剧情流程图"); }
    private void OnGUI()
    {
        graph = (StoryGraph)EditorGUILayout.ObjectField("剧情资源", graph, typeof(StoryGraph), false);
        if (!graph) { EditorGUILayout.HelpBox("选择 MainStory 或 DemoStory，查看分支与汇合。点击节点可在下方编辑。", MessageType.Info); return; }
        if (GUILayout.Button("检查所有节点连接"))
        {
            var errors = graph.Validate();
            EditorUtility.DisplayDialog("剧情校验", errors.Count == 0 ? "所有连接有效。" : string.Join("\n", errors), "关闭");
        }
        Rect viewport = GUILayoutUtility.GetRect(300, Mathf.Max(200, position.height * 0.5f));
        Rect content = new Rect(0, 0, 1050, Mathf.Ceil(graph.nodes.Count / 4f) * 110 + 30);
        scroll = GUI.BeginScrollView(viewport, scroll, content);
        Handles.BeginGUI();
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            foreach (string target in StoryGraph.Targets(graph.nodes[i]))
            {
                int j = graph.nodes.FindIndex(n => n.id == target);
                if (j < 0) continue;
                Rect from = NodeRect(i), to = NodeRect(j);
                var start = new Vector3(from.xMax, from.center.y);
                var end = new Vector3(to.x, to.center.y);
                Handles.DrawBezier(start, end, start + Vector3.right * 60, end + Vector3.left * 60, Color.cyan, null, 2);
                Handles.DrawLine(end, end + new Vector3(-7, -5));
                Handles.DrawLine(end, end + new Vector3(-7, 5));
            }
        }
        Handles.EndGUI();
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            GUI.backgroundColor = selected == i ? Color.cyan : Color.white;
            if (GUI.Button(NodeRect(i), graph.nodes[i].id + "\n" + graph.nodes[i].kind)) selected = i;
        }
        GUI.backgroundColor = Color.white;
        GUI.EndScrollView();
        if (selected >= 0 && selected < graph.nodes.Count)
        {
            var serialized = new SerializedObject(graph);
            serialized.Update();
            EditorGUILayout.PropertyField(serialized.FindProperty("nodes").GetArrayElementAtIndex(selected), true);
            serialized.ApplyModifiedProperties();
        }
    }
    private static Rect NodeRect(int index) => new Rect(20 + index % 4 * 260, 20 + index / 4 * 110, 180, 65);
}
