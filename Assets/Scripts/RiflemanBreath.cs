using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RiflemanBreath : MonoBehaviour
{

    public Sprite breath1, breath2, Fire;
    
    private Image img;
    private float B_Time = 0;
    private bool B1 = true;
    private bool B2 = false;
    private bool F = false;

    void Start()
    {
        img = this.transform.Find("img").GetComponent<Image>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            img.sprite = Fire;
            F = true;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            img.sprite = Fire;
            F = false;
        }

        if (B1 && !F)
        {
            img.sprite = breath1;
            B_Time += Time.deltaTime;
            Debug.Log("B1");
        }
        if(B2 && !F)
        {
            img.sprite = breath2;
            B_Time -= Time.deltaTime;
            Debug.Log("B2");
        }

        

        if (B_Time >= 1.0f)
        {
            B1 = false;
            B2 = true;
        }
        if(B_Time <= 0.0f)
        {
            B2 = false;
            B1 = true;
        }

        Debug.Log(B_Time);
    }
}
