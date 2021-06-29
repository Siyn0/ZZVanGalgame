using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPLoss : MonoBehaviour
{
    private Image img;
    private float Hp = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        img = this.transform.Find("img2").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Hp -= 0.1f;
            img.transform.localScale = new Vector3(Hp, 1, 1);
        }
    }
}
