using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Blur2test : MonoBehaviour
{
    public string SceneName;
    public Material material1;
    public Material material2;
    public float duration = 9.0f;
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();

        image.material = material1;
    }

    void FixedUpdate()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        image.material.Lerp(material1, material2, lerp);

        if (image.material.GetFloat("_blurSizeXY") < 0.05f)
        {
            image.enabled = false;
            //SceneManager.LoadScene(SceneName);
        }
    }

}
