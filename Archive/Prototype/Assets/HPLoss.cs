using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPLoss : MonoBehaviour
{
    private Image img;
    private float Hp = 1.0f;
    public ZZVan.Galgame.MinigameBridge result;

    // Start is called before the first frame update
    void Start()
    {
        img = this.transform.Find("img2").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0 || ZZVan.Galgame.GameOverlay.IsOpen) return;
        if (Hp > 0 && Input.GetKeyDown(KeyCode.F))
        {
            Hp = Mathf.Max(0, Hp - 0.1f);
            if (Hp < 0.001f) { Hp = 0; if (result) result.Succeed(); }
            img.transform.localScale = new Vector3(Hp, 1, 1);
        }
    }
}
