using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AddSan : MonoBehaviour
{
    public int SanAdd;
    public string SceneName;

    private int SanTemp;

    public void OnClick()
    {
        SanTemp = PlayerPrefs.GetInt("San_Temp");
        SanTemp += SanAdd;
        PlayerPrefs.SetInt("San_Temp", SanTemp);
        SceneManager.LoadScene(SceneName);
        Debug.Log("San+" + SanAdd.ToString());
        Debug.Log("San:" + PlayerPrefs.GetInt("San_Temp"));
    }
}
