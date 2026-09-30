using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using ZZVan.Galgame;
using Object = UnityEngine.Object;

public sealed class WorkbookStoryTests
{
    private DialogueWorkbook workbook;
    private MonoBehaviour Presenter => Object.FindObjectsOfType<MonoBehaviour>().Single(c => c.GetType().Name == "WorkbookDialoguePresenter");
    private T Field<T>(string name) => (T)Presenter.GetType().GetField(name).GetValue(Presenter);
    private int Row => (int)Presenter.GetType().GetProperty("CurrentRow").GetValue(Presenter);
    private bool Ended => (bool)Presenter.GetType().GetProperty("HasEnded").GetValue(Presenter);
    private void Go(int row) => Presenter.SendMessage("GoToExcelRow", row);
    private void Next() => Field<Button>("nextButton").onClick.Invoke();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ZZVAN_TEST_SAVE_DIRECTORY"))) Assert.Ignore("An isolated save directory is required.");
        Time.timeScale = 1;
        if (!GameSession.Instance) new GameObject("Test Session").AddComponent<GameSession>();
        GameSession.Instance.BeginSceneGame("测试姓名");
        yield return SceneManager.LoadSceneAsync("WorkbookStory");
        yield return null;
        workbook = Resources.Load<DialogueWorkbook>("DialogueWorkbook");
    }

    [Test]
    public void EveryBranchReachesAnEndpointWithoutCrossingSiblingRoutes()
    {
        Assert.IsEmpty(workbook.ValidateFlow());
        var visited = new HashSet<int>();
        var endpoints = new HashSet<int>();
        int paths = 0;
        void Walk(int id, HashSet<int> ancestors)
        {
            Assert.IsTrue(ancestors.Add(id), "Cycle at " + id);
            visited.Add(id);
            var row = workbook.Find(id);
            if (row.choices.Count > 0)
                foreach (var choice in row.choices) Walk(choice.targetRow, new HashSet<int>(ancestors));
            else if (row.nextRow > 0) Walk(row.nextRow, ancestors);
            else { endpoints.Add(id); paths++; }
        }
        Walk(2, new HashSet<int>());
        Assert.AreEqual(298, visited.Count);
        Assert.AreEqual(648, paths);
        CollectionAssert.AreEquivalent(new[] { 216, 237, 279, 299 }, endpoints);
        foreach (int row in new[] { 18, 25, 37 }) Assert.AreEqual(38, workbook.Next(workbook.Find(row)).excelRow);
        foreach (int row in new[] { 85, 89, 95 }) Assert.AreEqual(96, workbook.Next(workbook.Find(row)).excelRow);
    }

    [Test]
    public void OriginalButtonsDisplayAndDispatchThreeAndFourChoices()
    {
        Go(11);
        var buttons = Field<Button[]>("choiceButtons");
        Assert.AreEqual(3, buttons.Count(b => b.gameObject.activeSelf));
        Assert.IsFalse(buttons[0].gameObject.activeSelf);
        for (int i = 0; i < 3; i++)
        {
            Go(11);
            Assert.AreEqual(workbook.Find(11).choices[i].text, buttons[i + 1].GetComponentInChildren<Text>().text);
            buttons[i + 1].onClick.Invoke();
            Assert.AreEqual(new[] { 12, 19, 26 }[i], Row);
        }
        for (int i = 0; i < 4; i++)
        {
            Go(197);
            Assert.AreEqual(4, buttons.Count(b => b.gameObject.activeSelf));
            Assert.AreEqual(workbook.Find(197).choices[i].text, buttons[i].GetComponentInChildren<Text>().text);
            buttons[i].onClick.Invoke();
            Assert.AreEqual(new[] { 198, 217, 238, 280 }[i], Row);
        }
    }

    [Test]
    public void MissingArtworkClearsAndNameAndRedactionMarkersAreRendered()
    {
        var portrait = Field<Image>("portrait");
        portrait.sprite = Field<Button>("nextButton").image.sprite;
        portrait.enabled = true;
        Go(7);
        Assert.IsNull(portrait.sprite);
        Assert.IsFalse(portrait.enabled);
        Go(10);
        Assert.IsNull(Field<Image>("background").sprite);
        Assert.IsFalse(Field<Image>("background").enabled);
        Go(52);
        Assert.AreEqual("【违规描写】", Field<Text>("dialogueText").text);
        Go(164);
        Assert.That(Field<Text>("dialogueText").text, Does.Contain("测试姓名"));
        Assert.AreEqual("测试姓名", Field<Text>("characterName").text);
    }

    [UnityTest]
    public IEnumerator SaveRestoresRowBackdropAndVariablesWithoutRepeatingRewards()
    {
        Go(10);
        Go(25);
        Assert.AreEqual(20, GameSession.Instance.State.sisterAffection);
        int history = GameSession.Instance.History.Count;
        GameSession.Instance.Save(36);
        Next();
        Assert.AreEqual(38, Row);
        Go(148);
        Assert.IsTrue(GameSession.Instance.Load(36));
        yield return null;
        yield return null;
        Assert.AreEqual(25, Row);
        Assert.AreEqual(20, GameSession.Instance.State.sisterAffection);
        Assert.AreEqual(history, GameSession.Instance.History.Count);
        Assert.IsFalse(Field<Image>("background").enabled);
        Next();
        Assert.AreEqual(38, Row);
    }

    [UnityTest]
    public IEnumerator SaveAtChoiceRestoresAllOptionsAndPauseBlocksProgress()
    {
        Go(197);
        GameSession.Instance.Save(32);
        Presenter.SendMessage("Choose", 1);
        Assert.IsTrue(GameSession.Instance.Load(32));
        yield return null;
        yield return null;
        Assert.AreEqual(197, Row);
        var buttons = Field<Button[]>("choiceButtons");
        Assert.AreEqual(4, buttons.Count(b => b.gameObject.activeSelf));
        Time.timeScale = 0;
        buttons[0].onClick.Invoke();
        Assert.AreEqual(197, Row);
        Time.timeScale = 1;
        buttons[3].onClick.Invoke();
        Assert.AreEqual(280, Row);
    }

    [UnityTest]
    public IEnumerator ContentEndCanBeSavedRestoredAndReturnedToOriginalTitle()
    {
        Go(216);
        Next();
        Assert.IsTrue(Ended);
        Assert.That(Field<Text>("dialogueText").text, Does.Contain("当前表格剧情已结束"));
        GameSession.Instance.Save(31);
        Go(2);
        Assert.IsTrue(GameSession.Instance.Load(31));
        yield return null;
        yield return null;
        Assert.IsTrue(Ended);
        Next();
        yield return null;
        Assert.AreEqual("0", SceneManager.GetActiveScene().name);
    }
}
