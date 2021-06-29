using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    
    void Start()
    {
        PlayerPrefs.SetString("PlayerName_Temp","你好像没有名字");
        PlayerPrefs.SetInt("MeiMei_Temp",0);
        PlayerPrefs.SetInt("ZZ_Temp",0);
        PlayerPrefs.SetInt("Jue_Temp",0);
        PlayerPrefs.SetInt("San_Temp",0);
        PlayerPrefs.Save();
    }
}
