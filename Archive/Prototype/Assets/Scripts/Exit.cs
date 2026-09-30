using UnityEngine;
using UnityEngine.UI;
public class Exit : MonoBehaviour
{
    private void Start() { GetComponent<Button>().onClick.AddListener(OnClick); }
    public void OnClick()
    {
        ZZVan.Galgame.GameAudio.Instance.StopGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
