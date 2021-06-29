using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Dynamic;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public bool isPaused = false;
    public GameObject menu;

    private Image SaveLog;

    void Start()
    {
        SaveLog = menu.transform.Find("SaveLog").GetComponent<Image>();
        ContinueGame();
    }

    void Awake()
    {
        Pause();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }
    private void Pause()
    {
        isPaused = true;
        menu.SetActive(true);
        Time.timeScale = 0;
    }

    private void UnPause()
    {
        SaveLog.transform.localScale = new Vector3(0,0,0);
        isPaused = false;
        menu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ContinueGame()
    {
        UnPause();
    }

    public void SaveGame()
    {
        
        PlayerPrefs.SetString("PlayerName_Save", PlayerPrefs.GetString("PlayerName_Temp"));
        PlayerPrefs.SetString("SceneName",SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("San_Save",PlayerPrefs.GetInt("San_Temp"));
        PlayerPrefs.SetInt("MeiMei_Save",PlayerPrefs.GetInt("MeiMei_Temp"));
        PlayerPrefs.SetInt("ZZ_Save",PlayerPrefs.GetInt("ZZ_Temp"));
        PlayerPrefs.SetInt("Jue_Save",PlayerPrefs.GetInt("Jue_Temp"));

        Debug.Log("PlayerName_Save:"+PlayerPrefs.GetString("PlayerName_Save"));
        Debug.Log("SceneName:"+PlayerPrefs.GetString("SceneName"));
        SaveLog.transform.localScale = new Vector3(1,1,1);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        PlayerPrefs.SetString("PlayerName_Temp", PlayerPrefs.GetString("PlayerName_Save"));
        PlayerPrefs.SetInt("San_Temp",PlayerPrefs.GetInt("San_Save"));
        PlayerPrefs.SetInt("MeiMei_Temp",PlayerPrefs.GetInt("MeiMei_Save"));
        PlayerPrefs.SetInt("ZZ_Temp",PlayerPrefs.GetInt("ZZ_Save"));
        PlayerPrefs.SetInt("Jue_Temp",PlayerPrefs.GetInt("Jue_Save"));

        Debug.Log("PlayerName:"+PlayerPrefs.GetString("PlayerName_Save"));
        Debug.Log("PlayerName_Temp:"+PlayerPrefs.GetString("PlayerName_Temp"));
        SceneManager.LoadScene(PlayerPrefs.GetString("SceneName"));
    }

    public void MainMenu()
    {

        SceneManager.LoadScene("0");

    }

    
}

