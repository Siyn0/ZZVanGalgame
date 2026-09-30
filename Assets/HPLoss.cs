using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPLoss : MonoBehaviour, ZZVan.Galgame.ISceneCheckpoint
{
    private Image img;
    private float Hp = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        img = this.transform.Find("img2").GetComponent<Image>();
        if (ZZVan.Galgame.GameSession.Instance.SceneProgress.TryRestore(this, out HealthCheckpoint saved)) Hp = Mathf.Clamp01(saved.hp);
        img.transform.localScale = new Vector3(Hp, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0 || ZZVan.Galgame.GameSession.Instance.SceneProgress.IsRestoring) return;
        if (Hp > 0 && Input.GetKeyDown(KeyCode.F))
        {
            Hp = Mathf.Max(0, Hp - 0.1f);
            if (Hp < 0.001f) Hp = 0;
            img.transform.localScale = new Vector3(Hp, 1, 1);
        }
    }
    public string CaptureCheckpoint() => JsonUtility.ToJson(new HealthCheckpoint { hp = Hp });
    [System.Serializable] public sealed class HealthCheckpoint { public float hp; }
}
