using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZZVan.Galgame;

// Preserve these serialized fields and this script's GUID for existing scenes.
public class TextChange : MonoBehaviour, ISceneCheckpoint
{
    public string Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8, Text9, Text10;
    public string CText1, CText2, CText3, CText4, CText5, CText6, CText7, CText8, CText9, CText10;
    public Sprite CSprite1, CSprite2, CSprite3, CSprite4, CSprite5, CSprite6, CSprite7, CSprite8, CSprite9, CSprite10;
    public AudioClip Audio1, Audio2, Audio3, Audio4, Audio5, Audio6, Audio7, Audio8, Audio9, Audio10;
    public string LastText, SceneName;
    public List<DialogueLine> lines = new List<DialogueLine>();
    public string dialogueId;
    private Text body, speaker;
    private Image nameBackground, portrait;
    private int index;
    private readonly List<DialogueLine> runtimeLines = new List<DialogueLine>();

    private void Start()
    {
        body = Child<Text>("Text"); speaker = Child<Text>("CText");
        nameBackground = Child<Image>("CharacterBG"); portrait = Child<Image>("Girl");
        if (lines.Count > 0) runtimeLines.AddRange(lines);
        else
        {
            string[] texts = { Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8, Text9, Text10 };
            string[] names = { CText1, CText2, CText3, CText4, CText5, CText6, CText7, CText8, CText9, CText10 };
            Sprite[] sprites = { CSprite1, CSprite2, CSprite3, CSprite4, CSprite5, CSprite6, CSprite7, CSprite8, CSprite9, CSprite10 };
            AudioClip[] clips = { Audio1, Audio2, Audio3, Audio4, Audio5, Audio6, Audio7, Audio8, Audio9, Audio10 };
            int end = -1;
            for (int i = 0; i < texts.Length; i++) if (!string.IsNullOrEmpty(texts[i])) end = i;
            if (!string.IsNullOrEmpty(LastText))
                for (int i = 0; i <= end; i++) if (texts[i] == LastText) { end = i; break; }
            for (int i = 0; i <= end; i++) runtimeLines.Add(new DialogueLine
                { text = texts[i], speaker = names[i], portrait = sprites[i], voice = clips[i] });
        }
        bool resumed = GameSession.Instance.SceneProgress.TryRestore(this, out DialogueCheckpoint checkpoint);
        index = resumed ? checkpoint.index : 0;
        if (index >= runtimeLines.Count) index = 0;
        Show(resumed);
    }
    private T Child<T>(string child) where T : Component
    {
        var found = transform.Find(child);
        return found ? found.GetComponent<T>() : null;
    }
    public void onClick()
    {
        if (Time.timeScale == 0 || GameSession.Instance.SceneProgress.IsRestoring) return;
        if (index < runtimeLines.Count) index++;
        Show(false);
    }
    private void Show(bool resumed)
    {
        var session = GameSession.Instance;
        while (index < runtimeLines.Count && !resumed && !runtimeLines[index].condition.Evaluate(session.State)) index++;
        if (index >= runtimeLines.Count)
        {
            if (!string.IsNullOrWhiteSpace(SceneName) && Application.CanStreamedLevelBeLoaded(SceneName)) SceneManager.LoadScene(SceneName);
            return;
        }
        var line = runtimeLines[index];
        string name = session.State.Format((line.speaker ?? "").Replace("/PlayerName", "{playerName}"));
        string text = session.State.Format((line.text ?? "").Replace("/PlayerName", "{playerName}"));
        if (body) body.text = text;
        if (speaker) speaker.text = name;
        if (nameBackground) nameBackground.transform.localScale = string.IsNullOrEmpty(name) ? Vector3.zero : Vector3.one;
        if (portrait) { portrait.sprite = line.portrait; portrait.enabled = line.portrait != null; }
        if (!resumed)
        {
            foreach (var change in line.changes) change.Apply(session.State);
            foreach (string unlock in line.unlocks) session.Unlock(unlock);
            session.AddHistory(name, text);
            session.Unlock("achievement.first_dialogue");
            session.ObserveAsset(line.portrait);
            if (line.music) GameAudio.Instance.PlayMusic(line.music, line.loopMusic);
            if (line.fadeMusic) GameAudio.Instance.FadeOut(line.fadeSeconds);
        }
        if (!resumed) GameAudio.Instance.PlayVoice(VoiceSpeaker(line.speaker), line.voice);
    }
    public string CaptureCheckpoint() => JsonUtility.ToJson(new DialogueCheckpoint { index = index });
    [Serializable] public sealed class DialogueCheckpoint { public int index; }
    private static Speaker VoiceSpeaker(string name)
    {
        if (name == "纯乃" || name == "妹妹" || name == "妹") return Speaker.Sister;
        if (name == "古明地觉" || name == "觉") return Speaker.Satori;
        return string.Equals(name, "ZZ", StringComparison.OrdinalIgnoreCase) ? Speaker.ZZ : Speaker.Other;
    }
}
[Serializable]
public sealed class DialogueLine
{
    public string speaker;
    [TextArea] public string text;
    public Sprite portrait;
    public AudioClip voice, music;
    public bool loopMusic = true, fadeMusic;
    public float fadeSeconds = 1;
    public ConditionGroup condition = new ConditionGroup();
    public List<VariableChange> changes = new List<VariableChange>();
    public List<string> unlocks = new List<string>();
}
