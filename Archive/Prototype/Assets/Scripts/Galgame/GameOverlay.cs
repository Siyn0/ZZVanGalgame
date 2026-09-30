using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ZZVan.Galgame
{
    // Working fallback UI, available in legacy scenes without hand-editing scene YAML.
    public sealed class GameOverlay : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        private int tab;
        private Vector2 scroll;
        private string status = "";
        private float previousTimeScale;
        private EventSystem blockedEventSystem;
        private CollectionCatalog catalog;
        private CollectionEntry selected;
        private int pendingOverwrite;
        private readonly string[] tabs = { "存档 / 读档", "对话日志", "成就", "鉴赏", "结局" };
        private void Awake() { catalog = Resources.Load<CollectionCatalog>("GalgameCollection"); IsOpen = false; }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) Toggle();
        }
        public void Toggle()
        {
            if (IsOpen) { Close(); return; }
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0;
            blockedEventSystem = EventSystem.current;
            if (blockedEventSystem) blockedEventSystem.enabled = false;
            IsOpen = true;
        }
        private void Close()
        {
            IsOpen = false;
            Time.timeScale = previousTimeScale;
            if (blockedEventSystem) blockedEventSystem.enabled = true;
            pendingOverwrite = 0;
        }
        private void OnDestroy() { if (IsOpen) Close(); }
        private void OnGUI()
        {
            if (!IsOpen)
            {
                if (GUI.Button(new Rect(12, 12, 170, 30), "F1 · 存档 / 日志 / 收集")) Toggle();
                return;
            }
            float width = Mathf.Min(Screen.width - 24, 950), height = Screen.height - 24;
            GUILayout.BeginArea(new Rect(12, 12, width, height), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("游戏菜单");
            if (GUILayout.Button("关闭（F1）", GUILayout.Width(110))) Close();
            GUILayout.EndHorizontal();
            int next = GUILayout.Toolbar(tab, tabs);
            if (next != tab) { tab = next; scroll = Vector2.zero; selected = null; pendingOverwrite = 0; }
            GUILayout.Label(status);
            scroll = GUILayout.BeginScrollView(scroll);
            try
            {
                if (tab == 0) DrawSaves();
                else if (tab == 1)
                    foreach (var line in GameSession.Instance.History) GUILayout.Label(line.speaker + "：" + line.text);
                else DrawCollection();
            }
            catch (Exception e) { status = "操作失败：" + e.Message; }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        private void DrawSaves()
        {
            var session = GameSession.Instance;
            for (int slot = 1; slot <= SaveRepository.SlotCount; slot++)
            {
                var data = session.Saves.Read(slot);
                GUILayout.BeginHorizontal();
                GUILayout.Label(slot.ToString("D2") + " · " + (data == null ? "空档位" : data.savedAt + " · " + System.IO.Path.GetFileNameWithoutExtension(data.scene)));
                if (GUILayout.Button(pendingOverwrite == slot ? "确认覆盖" : "保存", GUILayout.Width(85)))
                {
                    if (data != null && pendingOverwrite != slot) pendingOverwrite = slot;
                    else { session.Save(slot); pendingOverwrite = 0; status = "已保存到档位 " + slot; }
                }
                GUI.enabled = data != null;
                if (GUILayout.Button("读取", GUILayout.Width(65)))
                {
                    if (session.Load(slot)) { Close(); Time.timeScale = 1; status = "读取成功"; }
                    else status = "无法读取：请检查存档和 Build Settings。";
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }
        private void DrawCollection()
        {
            if (!catalog) { GUILayout.Label("尚未配置收集目录：在 Resources 中创建 GalgameCollection 资源。"); return; }
            foreach (var entry in catalog.entries)
            {
                bool ending = entry.kind == CollectionKind.BadEnd || entry.kind == CollectionKind.NormalEnd || entry.kind == CollectionKind.TrueEnd;
                if (tab == 2 && entry.kind != CollectionKind.Achievement || tab == 4 && !ending ||
                    tab == 3 && (ending || entry.kind == CollectionKind.Achievement)) continue;
                bool unlocked = entry.availableFromStart || GameSession.Instance.IsUnlocked(entry.id);
                GUI.enabled = unlocked;
                if (GUILayout.Button(unlocked ? "★ " + entry.title : "☆ 未解锁")) selected = entry;
                GUI.enabled = true;
            }
            if (selected == null) return;
            GUILayout.Label(selected.title + "\n" + selected.description);
            if (selected.image) GUILayout.Label(selected.image.texture, GUILayout.MaxHeight(400));
            if (selected.music && GUILayout.Button("播放 / 循环")) GameAudio.Instance.PlayMusic(selected.music);
            if (selected.music && GUILayout.Button("淡出音乐")) GameAudio.Instance.FadeOut(1);
        }
    }
}
