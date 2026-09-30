using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RiflemanFire : MonoBehaviour
{
    public Sprite Fire;

    private Image img;

    void Start()
    {
        img = this.transform.Find("img").GetComponent<Image>();
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            img.sprite = Fire;
        }
    }
}
