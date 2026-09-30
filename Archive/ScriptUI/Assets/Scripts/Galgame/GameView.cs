using System;
using UnityEngine;

namespace ZZVan.Galgame
{
    // A self-contained UI keeps narrative data independent from scene hierarchies.
    public sealed class GameView : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        private int tab;
        private Vector2 scroll;
        private string playerName = "", status = "";
        private CollectionCatalog catalog;
        private CollectionEntry selected;
        private SaveData[] slots;
        private int overwrite;
        private MusicState previewReturn;
        private Font font;
        private GUIStyle body, title, small;
        private readonly string[] tabs = { "存档", "日志", "成就", "鉴赏", "结局", "流程" };
        private GameSession Session => GameSession.Instance;
        private void Awake()
        {
            catalog = Resources.Load<CollectionCatalog>("GalgameCollection");
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 22);
            IsOpen = false;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsOpen) Close(); else Open(0);
                return;
            }
            if (IsOpen || Session.Story == null) return;
            if (Session.Minigame != null)
            {
                var game = Session.Minigame;
                game.Tick(Time.deltaTime);
                if (game.id == "platformer") game.Move(Input.GetAxisRaw("Horizontal"), Input.GetKeyDown(KeyCode.Space), Time.deltaTime);
                if (game.finished) Session.Story.CompleteMinigame(game.won);
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) Run(() => Session.Story.Advance());
        }
        public void Open(int page)
        {
            tab = page; IsOpen = true; Time.timeScale = 0; scroll = Vector2.zero; status = "";
            RefreshSlots();
        }
        public void Close()
        {
            StopPreview(); IsOpen = false; Time.timeScale = 1; overwrite = 0;
        }
        private void OnDestroy()
        {
            IsOpen = false;
            if (font) Destroy(font);
        }
        private void StopPreview()
        {
            if (previewReturn == null) return;
            GameAudio.Instance.Restore(previewReturn); previewReturn = null;
        }
        private void RefreshSlots()
        {
            slots = new SaveData[SaveRepository.SlotCount];
            for (int i = 0; i < slots.Length; i++) slots[i] = Session.Saves.Read(i + 1);
        }
        private void Run(Action action)
        {
            try { action(); }
            catch (Exception e) { status = e.Message; Debug.LogException(e); }
        }
        private void OnGUI()
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            GUI.skin.font = font;
            GUI.skin.button.fontSize = 20; GUI.skin.label.fontSize = 20; GUI.skin.textField.fontSize = 22;
            if (body == null)
            {
                body = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 25, richText = false };
                title = new GUIStyle(body) { fontSize = 40 };
                small = new GUIStyle(body) { fontSize = 17 };
            }
            var node = Session.Story?.Current;
            if (node?.background) DrawSprite(new Rect(0, 0, 1280, 720), node.background, false);
            else { GUI.color = new Color(0.10f, 0.12f, 0.19f); GUI.DrawTexture(new Rect(0, 0, 1280, 720), Texture2D.whiteTexture); GUI.color = Color.white; }
            if (node?.portrait) DrawSprite(new Rect(720, 70, 500, 650), node.portrait, true);
            if (IsOpen) { DrawMenu(); return; }
            if (Session.Story == null) DrawTitle();
            else if (Session.Minigame != null) DrawMinigame();
            else DrawStory(node);
            if (GUI.Button(new Rect(1070, 20, 185, 42), "菜单 · F1")) Open(0);
            if (!string.IsNullOrEmpty(status)) GUI.Label(new Rect(25, 65, 1100, 45), status, small);
            if (!string.IsNullOrEmpty(Session.LastError)) GUI.Label(new Rect(25, 110, 1100, 45), Session.LastError, small);
        }
        private void DrawTitle()
        {
            GUI.Label(new Rect(100, 110, 900, 70), "ZZVan · 故事从这里开始", title);
            GUI.Label(new Rect(105, 205, 400, 35), "主角姓名", body);
            playerName = GUI.TextField(new Rect(105, 250, 400, 45), playerName, 30);
            if (GUI.Button(new Rect(105, 325, 400, 50), "开始故事"))
                Run(() => Session.StartStory(Resources.Load<StoryGraph>("MainStory"), playerName));
            if (GUI.Button(new Rect(105, 390, 400, 50), "读取进度")) Open(0);
            if (GUI.Button(new Rect(105, 455, 400, 50), "鉴赏与收集")) Open(3);
            if (GUI.Button(new Rect(105, 520, 400, 50), "系统演示 · 三种小游戏"))
                Run(() => Session.StartStory(Resources.Load<StoryGraph>("DemoStory"), playerName));
            if (GUI.Button(new Rect(105, 585, 400, 50), "退出"))
            {
                Session.ReturnToTitle();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
        private void DrawStory(StoryNode node)
        {
            GUI.Box(new Rect(30, 480, 1220, 215), "");
            GUI.Label(new Rect(65, 495, 1000, 35), Session.SpeakerName(node), body);
            GUI.Label(new Rect(65, 540, 1100, 100), Session.State.Format(node.text), body);
            if (node.kind == NodeKind.Dialogue && GUI.Button(new Rect(1030, 640, 180, 40), "继续 →"))
                Run(() => Session.Story.Advance());
            if (node.kind == NodeKind.End && GUI.Button(new Rect(1030, 640, 180, 40), "返回标题")) Session.ReturnToTitle();
            if (node.kind == NodeKind.Choice)
            {
                int row = 0;
                for (int i = 0; i < node.choices.Count; i++)
                {
                    var choice = node.choices[i];
                    if (!choice.condition.Evaluate(Session.State)) continue;
                    int selectedIndex = i;
                    if (GUI.Button(new Rect(240, 110 + row++ * 65, 800, 50), Session.State.Format(choice.text)))
                        Run(() => Session.Story.Choose(selectedIndex));
                }
                if (row == 0) GUI.Label(new Rect(240, 200, 800, 80), "当前没有可用选项，请读取先前进度。", body);
            }
        }
        private void DrawMenu()
        {
            GUI.Box(new Rect(20, 20, 1240, 680), "");
            GUI.Label(new Rect(50, 40, 500, 45), "游戏菜单", title);
            if (GUI.Button(new Rect(1080, 40, 145, 40), "返回")) Close();
            int next = GUI.Toolbar(new Rect(50, 105, 1175, 45), tab, tabs);
            if (next != tab) { StopPreview(); tab = next; scroll = Vector2.zero; selected = null; overwrite = 0; }
            GUI.Label(new Rect(50, 160, 1170, 30), status, small);
            GUILayout.BeginArea(new Rect(50, 200, 1175, 420));
            scroll = GUILayout.BeginScrollView(scroll);
            if (tab == 0) DrawSlots();
            else if (tab == 1)
                foreach (var line in Session.History) GUILayout.Label((string.IsNullOrEmpty(line.speaker) ? "" : line.speaker + "：") + line.text, body);
            else if (tab == 5) DrawFlow();
            else DrawCollection();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (GUI.Button(new Rect(50, 645, 200, 35), "返回标题")) { Close(); Session.ReturnToTitle(); }
            GUI.Label(new Rect(280, 645, 900, 35), "SAN " + Session.State.san + "    妹妹 " + Session.State.sisterAffection +
                "    觉 " + Session.State.satoriAffection + "    ZZ " + Session.State.zzAffection, small);
        }
        private void DrawSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int slot = i + 1; var data = slots[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(slot.ToString("D2") + " · " + (data == null ? "空档位" : data.savedAt + " · " + data.nodeId), small, GUILayout.Width(800));
                GUI.enabled = Session.Story != null;
                if (GUILayout.Button(overwrite == slot ? "确认覆盖" : "保存", GUILayout.Width(130)))
                {
                    if (data != null && overwrite != slot) overwrite = slot;
                    else Run(() => { Session.Save(slot); overwrite = 0; RefreshSlots(); status = "已保存"; });
                }
                GUI.enabled = data != null;
                if (GUILayout.Button("读取", GUILayout.Width(90)))
                    Run(() => { StopPreview(); if (Session.Load(slot)) { Close(); status = ""; } else status = "存档内容或对应剧情资源无效。"; });
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }
        private void DrawCollection()
        {
            if (!catalog) { GUILayout.Label("暂无收集内容。"); return; }
            foreach (var entry in catalog.entries)
            {
                bool ending = entry.kind == CollectionKind.BadEnd || entry.kind == CollectionKind.NormalEnd || entry.kind == CollectionKind.TrueEnd;
                if (tab == 2 && entry.kind != CollectionKind.Achievement || tab == 4 && !ending ||
                    tab == 3 && (ending || entry.kind == CollectionKind.Achievement)) continue;
                bool unlocked = entry.availableFromStart || Session.IsUnlocked(entry.id);
                GUI.enabled = unlocked;
                if (GUILayout.Button(unlocked ? "★ " + entry.title : "☆ 未解锁", GUILayout.Height(35))) selected = entry;
                GUI.enabled = true;
            }
            if (selected == null) return;
            GUILayout.Label(selected.title + "\n" + selected.description, body);
            if (selected.image) DrawSprite(GUILayoutUtility.GetRect(700, 350), selected.image, true);
            if (selected.music && GUILayout.Button("播放 / 循环", GUILayout.Height(40)))
                Run(() => { if (previewReturn == null) previewReturn = GameAudio.Instance.Capture(); GameAudio.Instance.PlayMusic(selected.music); });
            if (selected.music && GUILayout.Button("淡出", GUILayout.Height(40)))
                Run(() => { if (previewReturn == null) previewReturn = GameAudio.Instance.Capture(); GameAudio.Instance.FadeOut(1); });
        }
        private void DrawFlow()
        {
            var graph = Session.Story?.Graph ?? Resources.Load<StoryGraph>("MainStory");
            if (!graph) return;
            foreach (var node in graph.nodes)
            {
                bool visited = Session.IsUnlocked("node:" + graph.storyId + ":" + node.id);
                string edges = string.Join(" / ", StoryGraph.Targets(node));
                GUILayout.Label((visited ? "● " : "○ ") + node.id + " → " + (string.IsNullOrEmpty(edges) ? "结束" : edges) +
                    (visited ? "  " + node.kind : ""), small);
            }
        }
        private void DrawMinigame()
        {
            var game = Session.Minigame;
            GUI.Box(new Rect(90, 100, 1100, 540), "");
            GUI.Label(new Rect(120, 120, 850, 40), "剩余 " + Mathf.CeilToInt(game.remaining) + " 秒 · 得分 " + game.score, body);
            if (game.id == "shooting")
            {
                GUI.Label(new Rect(120, 175, 900, 40), "点击移动靶，命中 10 次获胜。", body);
                float elapsed = 30 - game.remaining;
                float x = 200 + Mathf.PingPong(elapsed * 170, 750), y = 290 + Mathf.Sin(elapsed * 2) * 90;
                if (GUI.Button(new Rect(x, y, 90, 90), "◎")) game.Hit();
            }
            else if (game.id == "memory")
            {
                GUI.Label(new Rect(120, 175, 900, 40), "翻开两张相同的牌，配对全部 4 组。", body);
                for (int i = 0; i < game.cards.Count; i++)
                {
                    GUI.enabled = !game.matched.Contains(i);
                    string text = game.matched.Contains(i) || game.selected == i || game.revealed == i ? (game.cards[i] + 1).ToString() : "?";
                    if (GUI.Button(new Rect(270 + i % 4 * 180, 260 + i / 4 * 130, 130, 100), text)) game.Flip(i);
                }
                GUI.enabled = true;
            }
            else
            {
                GUI.Label(new Rect(120, 175, 950, 40), "A / D 移动，空格跳跃；越过两个缺口，到达右侧旗帜。", body);
                GUI.Box(new Rect(140, 550, 960, 30), "");
                GUI.Box(new Rect(140 + 960 * 0.32f, 550, 960 * 0.08f, 55), "缺口");
                GUI.Box(new Rect(140 + 960 * 0.65f, 550, 960 * 0.08f, 55), "缺口");
                GUI.Box(new Rect(140 + game.x * 960, 515 - game.y * 350, 30, 35), "人");
                GUI.Label(new Rect(1070, 510, 60, 40), "旗", body);
            }
            if (GUI.Button(new Rect(930, 580, 200, 40), "放弃本次挑战")) Session.Story.CompleteMinigame(false);
        }
        private static void DrawSprite(Rect area, Sprite sprite, bool fit)
        {
            var rect = sprite.textureRect;
            if (fit)
            {
                float scale = Mathf.Min(area.width / rect.width, area.height / rect.height);
                area = new Rect(area.center.x - rect.width * scale / 2, area.yMax - rect.height * scale, rect.width * scale, rect.height * scale);
            }
            GUI.DrawTextureWithTexCoords(area, sprite.texture, new Rect(rect.x / sprite.texture.width, rect.y / sprite.texture.height,
                rect.width / sprite.texture.width, rect.height / sprite.texture.height));
        }
    }
}
