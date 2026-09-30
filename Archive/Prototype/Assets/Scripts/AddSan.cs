using UnityEngine;
using UnityEngine.SceneManagement;
using ZZVan.Galgame;
public class AddSan : MonoBehaviour
{
    public int SanAdd;
    public string SceneName;
    public void OnClick()
    {
        GameSession.Instance.State.san += SanAdd;
        GameSession.Instance.SyncLegacyState();
        SceneManager.LoadScene(SceneName);
    }
}
