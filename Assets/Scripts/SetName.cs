using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZZVan.Galgame;
public class SetName : MonoBehaviour
{
    public InputField input;
    private void Start() { GetComponent<Button>().onClick.AddListener(OnClick); }
    public void OnClick()
    {
        if (!input) input = FindObjectOfType<InputField>();
        GameSession.Instance.BeginSceneGame(input ? input.text : "主角");
        SceneManager.LoadScene("WorkbookStory");
    }
}
