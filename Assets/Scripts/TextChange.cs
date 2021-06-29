using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class TextChange : MonoBehaviour
{
    public string Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8, Text9, Text10;    //说出的文本
    public string CText1, CText2, CText3, CText4, CText5, CText6, CText7, CText8, CText9, CText10;    //角色名字（可选）
    public Sprite CSprite1, CSprite2, CSprite3, CSprite4, CSprite5, CSprite6, CSprite7, CSprite8, CSprite9, CSprite10;    //角色立绘（可选） 
    public AudioClip Audio1, Audio2, Audio3, Audio4, Audio5, Audio6, Audio7, Audio8, Audio9, Audio10;    //配音（可选）
    public string LastText;    //最后一句话
    public string SceneName;    //下一个场景的名字

    //------------------------------------------

    private Text NowText;    //当前文本
    private Text CText;    //角色名字

    private String PlayerName;    //如果对话中包括男主说的话，这个变量用于在加载时读取玩家名字

    private Image CharacterBG, Girl;    //角色名字框和立绘

    private AudioSource AudioS;    //角色说话的配音播放器

    void Start()
    {
        //this.GetComponent<Button>().onClick.AddListener(onClick);
        try
        {
            NowText = this.transform.Find("Text").GetComponent<Text>();
        }
        catch(Exception)
        {
            
        }
        try
        {
            CText = this.transform.Find("CText").GetComponent<Text>();
        }
        catch(Exception)
        {
            
        }
        try
        {
            CharacterBG = this.transform.Find("CharacterBG").GetComponent<Image>();
        }
        catch (Exception)
        {
            
            
        }

        PlayerName = PlayerPrefs.GetString("PlayerName_Temp");

        AudioS = gameObject.AddComponent<AudioSource>();
        AudioS.playOnAwake = false;


        try
        {
            AudioS.clip = Audio1;
            AudioS.Play();
        }
        catch (Exception)
        {
            
            
        }
        
        try
        {
            Girl = this.transform.Find("Girl").GetComponent<Image>();
        }
        catch(Exception)
        {

        }
        
        try
        {
            if(CText1 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
            {
                CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
            }
            else
            {
                CText.text = CText1;
            }
        }
        catch(Exception)
        {

        }

        if(CText1 == "")
        {
            CharacterBG.transform.localScale = new Vector3(0,0,0);// 原：CharacterBG.enabled= false;
        }
        else
        {
            CharacterBG.transform.localScale = new Vector3(1,1,1);
        }

        NowText.text = Text1;
        try
        {
            Girl.sprite = CSprite1;
        }
        catch(Exception)
        {

        }
        
    }


    public void onClick()
    {
        PlayerName = PlayerPrefs.GetString("PlayerName_Temp");    //这行用于解决读档之后玩家名获取不到的问题

        if(NowText.text == LastText)
        {
            SceneManager.LoadScene(SceneName);
        }

        //9to10
        if(NowText.text == Text9)
        {
            NowText.text = Text10;
            
            try
            {
                if(CText10 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText10;
                }
                
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite10;
            }
            catch(Exception)
            {

            }

            if(CText10 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio10;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //8to9
        if(NowText.text == Text8)
        {
            NowText.text = Text9;
            
            try
            {
                if(CText9 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText9;
                }
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite9;
            }
            catch(Exception)
            {

            }

            if(CText9 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio9;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //7to8
        if(NowText.text == Text7)
        {
            NowText.text = Text8;
            
            try
            {
                if(CText8 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText8;
                }
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite8;
            }
            catch(Exception)
            {

            }

            if(CText8 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio8;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //6to7
        if(NowText.text == Text6)
        {
            NowText.text = Text7;
            
            try
            {
                if(CText7 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText7;
                }
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite7;
            }
            catch(Exception)
            {

            }

            if(CText7 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio7;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //5to6
        if(NowText.text == Text5)
        {
            NowText.text = Text6;
            
            try
            {
                if(CText6 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText6;
                }
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite6;
            }
            catch(Exception)
            {

            }

            if(CText6 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }

            try
            {
                AudioS.Pause();
                AudioS.clip = Audio6;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //4to5
        if(NowText.text == Text4)
        {
            NowText.text = Text5;
            
            try
            {
                if(CText5 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText5;
                }
            }
            catch(Exception)
            {

            }
            
            try
            {
                Girl.sprite = CSprite5;
            }
            catch(Exception)
            {

            }

            if(CText5 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio5;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //3to4
        if(NowText.text == Text3)
        {
            NowText.text = Text4;
            
            try
            {
                if(CText4 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText4;
                }
            }
            catch(Exception)
            {

            }

            try
            {
                Girl.sprite = CSprite4;
            }
            catch(Exception)
            {

            }

            if(CText4 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio4;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //2to3
        if(NowText.text == Text2)
        {
            NowText.text = Text3;
            
            try
            {
                if(CText3 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText3;
                }
            }
            catch(Exception)
            {

            }

            try
            {
                Girl.sprite = CSprite3;
            }
            catch(Exception)
            {

            }

            if(CText3 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio3;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }

        //1to2
        if(NowText.text == Text1)
        {
            NowText.text = Text2;
            
            try
            {
                if(CText2 == "/PlayerName")    //如果在Unity属性面板输入的CText为“/PlayerName”
                {
                    CText.text = PlayerName;    //那么角色名字显示为最初加载场景时获取到的玩家名字
                }
                else
                {
                    CText.text = CText2;
                }
            }
            catch(Exception)
            {

            }
            try
            {
                Girl.sprite = CSprite2;
            }
            catch(Exception)
            {

            }
            if(CText2 == "")
            {
                CharacterBG.transform.localScale = new Vector3(0,0,0);
            }
            else
            {
                CharacterBG.transform.localScale = new Vector3(1,1,1);
            }
            
            try
            {
                AudioS.Pause();
                AudioS.clip = Audio2;
                AudioS.Play();
            }
            catch (Exception)
            {
            
            
            }
        }
        
        
    }


}
