using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZZVan.Galgame;
public class AddMeiPoint : MonoBehaviour
{
    public int MeiAdd;
    public string SceneName;
    private void Start() { GetComponent<Button>().onClick.AddListener(OnClick); }
    public void OnClick()
    {
        GameSession.Instance.State.sisterAffection += MeiAdd;
        SceneManager.LoadScene(SceneName);
    }
}
