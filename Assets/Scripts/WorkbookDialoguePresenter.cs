using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZZVan.Galgame;

// Bind this to hand-authored scene objects. It never creates a Canvas, panel or button.
public sealed class WorkbookDialoguePresenter : MonoBehaviour, ISceneCheckpoint
{
    public DialogueWorkbook workbook;
    public string sequence = "开场醒来";
    public Text dialogueText, characterName;
    public Image portrait, background, nameBackground;
    public Button nextButton;
    public string completionScene = "0";
    public Button[] choiceButtons = new Button[0];
    public UnityEvent onSequenceFinished = new UnityEvent();
    public WorkbookEffectEvent onEffect = new WorkbookEffectEvent();
    private WorkbookRow current;
    private bool ended;
    private int backgroundRow;
    private UnityAction[] listeners;
    public int CurrentRow => current?.excelRow ?? 0;
    public bool HasEnded => ended;
    // The first slot is the extra upper button; three-choice scenes keep their original placement.
    private int FirstChoiceSlot => Math.Max(0, choiceButtons.Length - Math.Max(3, current?.choices.Count ?? 0));
    private void Start()
    {
        if (!workbook) workbook = Resources.Load<DialogueWorkbook>("DialogueWorkbook");
        if (!workbook) { Debug.LogError("请先导入 DialogueTemplate.xlsx。", this); enabled = false; return; }
        listeners = new UnityAction[choiceButtons.Length];
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            listeners[i] = () => Choose(index - FirstChoiceSlot);
            if (choiceButtons[i]) choiceButtons[i].onClick.AddListener(listeners[i]);
        }
        bool restored = GameSession.Instance.SceneProgress.TryRestore(this, out Checkpoint saved);
        current = restored ? workbook.Find(saved.row) : workbook.First(sequence);
        ended = restored && saved.ended;
        backgroundRow = restored ? saved.backgroundRow : 0;
        if (current == null) { Debug.LogError("找不到指定的 Excel 段落或存档行。", this); enabled = false; return; }
        if (backgroundRow > 0) ApplyBackground(workbook.Find(backgroundRow));
        Show(restored);
    }
    public void Next()
    {
        if (!CanInteract()) return;
        if (ended)
        {
            if (!string.IsNullOrEmpty(completionScene))
            {
                GameSession.Instance.ReturnToTitle();
                SceneManager.LoadScene(completionScene);
            }
            return;
        }
        if (current.choices.Count > 0) return;
        var next = workbook.Next(current);
        if (next != null) { current = next; Show(false); }
        else { ended = true; onSequenceFinished.Invoke(); if (ended) ShowEnd(); }
    }
    public void Choose(int index)
    {
        if (!CanInteract() || ended || index < 0 || index >= current.choices.Count) return;
        GoToExcelRow(current.choices[index].targetRow);
    }
    public void GoToExcelRow(int row)
    {
        var next = workbook ? workbook.Find(row) : null;
        if (next == null) { Debug.LogError("无效的 Excel 行号：" + row, this); return; }
        current = next; ended = false; Show(false);
    }
    private bool CanInteract() => enabled && current != null && Time.timeScale > 0 && !GameSession.Instance.SceneProgress.IsRestoring;
    private void Show(bool restored)
    {
        var session = GameSession.Instance;
        string speaker = current.character == "主角" ? session.State.playerName : current.character == "旁白" ? "" : current.character;
        string text = session.State.Format(current.text);
        if (dialogueText) dialogueText.text = text;
        if (characterName) characterName.text = speaker;
        if (nameBackground) nameBackground.enabled = !string.IsNullOrEmpty(speaker);
        if (!string.IsNullOrWhiteSpace(current.backgroundFile)) ApplyBackground(current);
        // Missing artwork must not leave the previous character visible.
        if (portrait)
        {
            portrait.sprite = current.portraitFile == "【占位符】" ? null : current.portrait;
            portrait.enabled = portrait.sprite != null;
        }
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (!choiceButtons[i]) continue;
            int option = i - FirstChoiceSlot;
            bool visible = !ended && option >= 0 && option < current.choices.Count;
            choiceButtons[i].gameObject.SetActive(visible);
            if (visible)
            {
                var label = choiceButtons[i].GetComponentInChildren<Text>(true);
                if (label) label.text = session.State.Format(current.choices[option].text);
            }
        }
        if (nextButton) nextButton.interactable = current.choices.Count == 0;
        if (ended) { ShowEnd(); return; }
        if (current.choices.Count > choiceButtons.Length) Debug.LogError("场景中绑定的选项按钮数量不足，Excel 行 " + current.excelRow, this);
        if (restored) return;
        foreach (var change in current.changes) change.Apply(session.State);
        session.AddHistory(speaker, text);
        session.Unlock("achievement.first_dialogue");
        Speaker voice = current.character == "纯乃" || current.character == "妹妹" ? Speaker.Sister :
            current.character == "古明地觉" || current.character == "觉" ? Speaker.Satori :
            string.Equals(current.character, "ZZ", StringComparison.OrdinalIgnoreCase) ? Speaker.ZZ : Speaker.Other;
        if (voice == Speaker.Sister) session.Unlock("character.sister");
        if (voice == Speaker.Satori) session.Unlock("character.satori");
        if (voice == Speaker.ZZ) session.Unlock("character.zz");
        if (portrait) session.ObserveAsset(portrait.sprite);
        GameAudio.Instance.PlayVoice(voice, current.voiceFile == "【占位符】" ? null : current.voice);
        if (!string.IsNullOrWhiteSpace(current.effect)) onEffect.Invoke(current.effect, current.effectDuration);
    }
    private void ApplyBackground(WorkbookRow row)
    {
        if (row == null || string.IsNullOrWhiteSpace(row.backgroundFile)) return;
        backgroundRow = row.excelRow;
        if (!background) return;
        background.sprite = row.backgroundFile == "【占位符】" ? null : row.background;
        background.enabled = background.sprite != null;
        GameSession.Instance.ObserveAsset(background.sprite);
    }
    private void ShowEnd()
    {
        if (dialogueText) dialogueText.text = "当前表格剧情已结束。\n点击返回主菜单。";
        if (characterName) characterName.text = "";
        if (nameBackground) nameBackground.enabled = false;
        if (portrait) { portrait.sprite = null; portrait.enabled = false; }
        foreach (var button in choiceButtons) if (button) button.gameObject.SetActive(false);
        if (nextButton) nextButton.interactable = true;
    }
    public string CaptureCheckpoint() => JsonUtility.ToJson(new Checkpoint { row = current?.excelRow ?? 0, ended = ended, backgroundRow = backgroundRow });
    private void OnDestroy()
    {
        if (listeners == null) return;
        for (int i = 0; i < listeners.Length; i++) if (choiceButtons[i]) choiceButtons[i].onClick.RemoveListener(listeners[i]);
    }
    [Serializable] public sealed class Checkpoint { public int row, backgroundRow; public bool ended; }
    [Serializable] public sealed class WorkbookEffectEvent : UnityEvent<string, float> { }
}
