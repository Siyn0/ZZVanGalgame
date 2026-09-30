using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZZVan.Galgame;
public class GameManager : MonoBehaviour
{
    public bool isPaused;
    public GameObject menu;
    [Range(1, SaveRepository.SlotCount)] public int selectedSlot = 1;
    private Transform saveLog;
    private void Start()
    {
        var session = GameSession.Instance;
        if (menu) saveLog = menu.transform.Find("SaveLog");
        ContinueGame();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ContinueGame();
            else { isPaused = true; if (menu) menu.SetActive(true); Time.timeScale = 0; }
        }
    }
    public void ContinueGame()
    {
        if (saveLog) saveLog.localScale = Vector3.zero;
        isPaused = false;
        if (menu) menu.SetActive(false);
        Time.timeScale = 1;
    }
    public void SaveGame()
    {
        try { GameSession.Instance.Save(selectedSlot); if (saveLog) saveLog.localScale = Vector3.one; }
        catch (Exception e) { Debug.LogError("存档失败：" + e.Message); }
    }
    public void SelectSlot(int slot) { selectedSlot = Mathf.Clamp(slot, 1, SaveRepository.SlotCount); }
    public void LoadGame()
    {
        if (!GameSession.Instance.Load(selectedSlot)) Debug.LogWarning("档位为空、损坏，或存档场景未加入 Build Settings。");
    }
    public void MainMenu()
    {
        ContinueGame(); GameSession.Instance.ReturnToTitle(); SceneManager.LoadScene("0");
    }
}
