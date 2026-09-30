using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ZZVan.Galgame;

public sealed class SessionTests
{
    private GameSession session;
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("ZZVAN_TEST_SAVE_DIRECTORY")))
            Assert.Ignore("Set ZZVAN_TEST_SAVE_DIRECTORY to an isolated directory before running persistence tests.");
        session = GameSession.Instance;
        if (!session) session = new GameObject("Test Session").AddComponent<GameSession>();
        session.StartStory(Resources.Load<StoryGraph>("DemoStory"), "测试主角");
        yield return null;
    }
    [TearDown]
    public void TearDown() { if (session) session.ReturnToTitle(); }

    [UnityTest]
    public IEnumerator LoadRestoresNodeWithoutRepeatingRewardsOrHistory()
    {
        session.Story.Enter("won");
        int history = session.History.Count;
        session.Save(1);
        session.State.san = 99;
        session.Unlock("test.permanent.after.save");
        Assert.IsTrue(session.Load(1));
        Assert.AreEqual("won", session.Story.Current.id);
        Assert.AreEqual(1, session.State.san);
        Assert.AreEqual(history, session.History.Count);
        Assert.IsTrue(session.IsUnlocked("test.permanent.after.save"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator LogRetainsLatestFiftyAndNameIsSubstituted()
    {
        Assert.That(session.History[0].text, Does.Contain("测试主角"));
        for (int i = 0; i < 60; i++) session.AddHistory("妹妹", "line-" + i);
        Assert.AreEqual(50, session.History.Count);
        Assert.AreEqual("line-10", session.History[0].text);
        Assert.AreEqual("line-59", session.History[49].text);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MinigameSaveRestoresPartialRound()
    {
        session.Story.Enter("memory");
        session.Minigame.Flip(2);
        session.Minigame.Tick(7);
        int card = session.Minigame.cards[2];
        session.Save(36);
        session.Story.CompleteMinigame(false);
        Assert.IsTrue(session.Load(36));
        Assert.AreEqual("memory", session.Story.Current.id);
        Assert.AreEqual(2, session.Minigame.selected);
        Assert.AreEqual(card, session.Minigame.cards[2]);
        Assert.That(session.Minigame.remaining, Is.EqualTo(53).Within(0.1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator AudioChannelsRemainIndependentAcrossSilentDialogueAndSceneChange()
    {
        var audio = GameAudio.Instance;
        var sources = audio.GetComponents<AudioSource>();
        Assert.AreEqual(2, sources.Length);
        var voiceClip = AudioClip.Create("voice-test", 88200, 1, 44100, false);
        var otherVoice = AudioClip.Create("voice-next", 88200, 1, 44100, false);
        var music = Resources.Load<AudioLibrary>("GalgameAudioLibrary").entries[0].clip;
        audio.PlayVoice(Speaker.Sister, voiceClip);
        audio.PlayMusic(music);
        audio.PlayVoice(Speaker.Narrator, otherVoice);
        audio.PlayVoice(Speaker.ZZ, null);
        Assert.AreEqual(voiceClip, sources[1].clip);
        Assert.AreEqual(music, sources[0].clip);
        var temporary = UnityEngine.SceneManagement.SceneManager.CreateScene("Audio persistence test");
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(temporary);
        yield return null;
        Assert.AreEqual(voiceClip, sources[1].clip);
        audio.PlayVoice(Speaker.Satori, otherVoice);
        Assert.AreEqual(otherVoice, sources[1].clip);
        Time.timeScale = 0;
        audio.FadeOut(0.05f);
        yield return new WaitForSecondsRealtime(0.15f);
        Assert.IsFalse(sources[0].isPlaying);
        Assert.AreEqual(otherVoice, sources[1].clip);
        Time.timeScale = 1;
        audio.StopGame();
        Object.Destroy(voiceClip); Object.Destroy(otherVoice);
        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(temporary);
    }

    [UnityTest]
    public IEnumerator OriginalBranchesKeepTheirReturnPointsAndVariableEffects()
    {
        var graph = Resources.Load<StoryGraph>("MainStory");
        for (int branch = 0; branch < 3; branch++)
        {
            session.StartStory(graph, "测试");
            int steps = 0;
            while (session.Story.Current.kind == NodeKind.Dialogue && steps++ < 100) session.Story.Advance();
            Assert.IsTrue(session.Story.Choose(branch));
            while (session.Story.Current.kind == NodeKind.Dialogue && steps++ < 100) session.Story.Advance();
            Assert.AreEqual(branch == 1 ? "excerpt.end" : "3_Chose.choice", session.Story.Current.id);
            Assert.AreEqual(branch == 1 ? 20 : 0, session.State.sisterAffection);
            Assert.AreEqual(branch == 2 ? 1 : 0, session.State.san);
            if (branch != 1)
            {
                Assert.IsTrue(session.Story.Choose(1));
                while (session.Story.Current.kind == NodeKind.Dialogue && steps++ < 100) session.Story.Advance();
                Assert.AreEqual("excerpt.end", session.Story.Current.id);
            }
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator SaveLoadRestoresMusicAndVoiceClips()
    {
        session.StartStory(Resources.Load<StoryGraph>("MainStory"), "测试");
        session.Story.Enter("1.6");
        var sources = GameAudio.Instance.GetComponents<AudioSource>();
        var savedMusic = sources[0].clip;
        var savedVoice = sources[1].clip;
        yield return null;
        session.Save(2);
        GameAudio.Instance.StopGame();
        Assert.IsTrue(session.Load(2));
        Assert.AreEqual(savedMusic, sources[0].clip);
        Assert.AreEqual(savedVoice, sources[1].clip);
        Assert.AreEqual("1.6", session.Story.Current.id);
    }
}
