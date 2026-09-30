using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using ZZVan.Galgame;

public sealed class OriginalSceneTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("ZZVAN_TEST_SAVE_DIRECTORY")))
            Assert.Ignore("An isolated save directory is required.");
        Time.timeScale = 1;
        yield return SceneManager.LoadSceneAsync("0");
        yield return null;
        GameSession.Instance.BeginSceneGame("场景测试");
    }
    private static MonoBehaviour Find(string type) => Object.FindObjectsOfType<MonoBehaviour>().FirstOrDefault(c => c.GetType().Name == type);

    [UnityTest]
    public IEnumerator OriginalTitleOpensSeparateNamingScene()
    {
        Assert.IsNull(Object.FindObjectOfType<InputField>(), "Title should not contain a naming input.");
        Assert.IsFalse(Object.FindObjectsOfType<MonoBehaviour>().Any(c => c.GetType().Name == "GameView"));
        Assert.Greater(Object.FindObjectsOfType<Image>().Count(i => i.sprite), 3);
        GameObject.Find("StartButton").GetComponent<Button>().onClick.Invoke();
        yield return null;
        Assert.AreEqual("0SetName", SceneManager.GetActiveScene().name);
        var input = Object.FindObjectOfType<InputField>();
        Assert.IsNotNull(input);
        input.text = "原来的命名页面";
        Find("SetName").SendMessage("OnClick");
        yield return null;
        Assert.AreEqual("WorkbookStory", SceneManager.GetActiveScene().name);
        Assert.IsNotNull(Find("WorkbookDialoguePresenter"));
        Assert.AreEqual("原来的命名页面", GameSession.Instance.State.playerName);
    }

    [UnityTest]
    public IEnumerator AuthoredDialogueRestoresExactLineAndDoesNotDuplicateHistory()
    {
        yield return SceneManager.LoadSceneAsync("1");
        yield return null;
        Find("TextChange").SendMessage("onClick");
        var expected = ((ISceneCheckpoint)Find("TextChange")).CaptureCheckpoint();
        int count = GameSession.Instance.History.Count;
        GameSession.Instance.Save(35);
        Find("TextChange").SendMessage("onClick");
        Assert.IsTrue(GameSession.Instance.Load(35));
        yield return null;
        yield return null;
        Assert.AreEqual(expected, ((ISceneCheckpoint)Find("TextChange")).CaptureCheckpoint());
        Assert.AreEqual(count, GameSession.Instance.History.Count);
        var manager = Find("GameManager");
        if (manager) { manager.SendMessage("ContinueGame"); Assert.AreEqual(1, Time.timeScale); }
    }

    [UnityTest]
    public IEnumerator OriginalPlatformGameAndItsSpriteArePresent()
    {
        yield return SceneManager.LoadSceneAsync("playDemo");
        yield return null;
        var player = Find("PlayerController");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Rigidbody2D>());
        Assert.IsNotNull(player.GetComponent<SpriteRenderer>().sprite);
        Assert.Greater(Object.FindObjectsOfType<Collider2D>().Length, 1);
        GameSession.Instance.Save(34);
        var snapshot = ((ISceneCheckpoint)player).CaptureCheckpoint();
        Assert.IsTrue(GameSession.Instance.Load(34));
        yield return null;
        yield return null;
        Assert.IsNotNull(Find("PlayerController"));
        Assert.That(GameSession.Instance.Saves.Read(34).sceneCheckpoints[0].json, Is.EqualTo(snapshot));
    }

    [UnityTest]
    public IEnumerator OriginalDotaGameRetainsHealthAndAnimationComponents()
    {
        yield return SceneManager.LoadSceneAsync("dlc_DotA");
        yield return null;
        Assert.IsNotNull(Find("HPLoss"));
        Assert.IsNotNull(Find("RiflemanBreath"));
        Assert.IsNotNull(Find("RSBreath"));
        Assert.Greater(Object.FindObjectsOfType<Image>().Count(i => i.sprite), 3);
        GameSession.Instance.Save(33);
        Assert.IsTrue(GameSession.Instance.Load(33));
        yield return null;
        yield return null;
        Assert.IsNotNull(Find("HPLoss"));
    }
}
