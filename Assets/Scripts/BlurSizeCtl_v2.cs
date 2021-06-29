using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurSizeCtl_v2 : MonoBehaviour
{
    public UnityEngine.UI.Image FadeImage;
    private UnityEngine.UI.Image clone;
    void Start()
    {

        clone = Instantiate(FadeImage);
        clone.material.SetFloat("_blurSizeXY", 10.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (clone.material.GetFloat("_blurSizeXY") >= 0.05f)
        {
            clone.material.SetFloat("_blurSizeXY", clone.material.GetFloat("_blurSizeXY") - 0.1f);
        }

        if (clone.material.GetFloat("_blurSizeXY") < 0.05f)
        {
            clone.material.SetFloat("_blurSizeXY", 0);
            Destroy(clone);
        }
    }
}
