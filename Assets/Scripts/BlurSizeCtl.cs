using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class BlurSizeCtl : MonoBehaviour
{
    public GameObject GO;
    public Image image;
    public string SceneName;
    public Material material1, materialINIT;
    public Material material2;

    public float fadeSpeed = 1f;

    public float duration = 10.0f;
    public bool sceneStarting = true;

    private float lerp;




    void Start()
    {
        
        
        //image.enabled = true;
        //image.material.Lerp(material1, materialINIT, 1);
        //material1 = materialINIT;
        //image.material = material1;
        //尝试修复重新加载场景时卡住
        //image.material.SetFloat("_blurSizeXY", 10.0f);

    }
    
    void Awake() 
    {
        sceneStarting = true;
        image.material.SetFloat("_blurSizeXY", 10.0f);
    }

    void FixedUpdate()
    {
        if (sceneStarting)
        {
            //StartVagueScene();
            //if(image.material.GetFloat("_blurSizeXY") >= 0.05f)
            //{
                image.material.SetFloat("_blurSizeXY",image.material.GetFloat("_blurSizeXY")-0.1f);
            //}
            /*
            if (image.material.GetFloat("_blurSizeXY") < 0.05f)
            {
                //image.material.SetFloat("_blurSizeXY", 0);
                image.material.SetFloat("_blurSizeXY", 10.0f);
                image.enabled = false;
                sceneStarting = false;
            }
            */
        }
        if (image.material.GetFloat("_blurSizeXY") < 0.05f)
        {
            image.enabled = false;
            sceneStarting = false;
            //SceneManager.LoadScene(SceneName);
        }
        /*
        else
        {
            image.enabled = true;
            image.material.SetFloat("_blurSizeXY", 10.0f);
        }
        */
    }

    /*
    private void VagueToClear()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        image.material.Lerp(material1, material2, lerp);
    }
    */
    
    
    
    /*
    void FadeToBlack()
    {
        image.color = Color.Lerp(image.color, Color.black, fadeSpeed * Time.deltaTime);
    }
    */
    

    /*
    void StartVagueScene()
    {
        //VagueToClear();

        //重新编写的模糊渐变：
        image.material.SetFloat("_blurSizeXY",image.material.GetFloat("_blurSizeXY")-0.1f);

        if (image.material.GetFloat("_blurSizeXY") < 0.05f)
        {
            image.material.SetFloat("_blurSizeXY", 0);
            image.enabled = false;
            sceneStarting = false;
        }
    }
    */

    
    void EndScene()
    {
        image.enabled = true;
        sceneStarting = true;
        image.material.SetFloat("_blurSizeXY", 10.0f);
        //FadeToBlack();

        /*
        if (image.material.GetFloat("_blurSizeXY") < 0.05f)
        {
            image.enabled = false;
            sceneStarting = false;
            SceneManager.LoadScene(SceneName);
        }
        */
    }
    
    /*
    void OnDestroy()
    {
        Debug.Log("OnDestroy1");
        Destroy(GO);    //气急败坏
    }
    */
}
