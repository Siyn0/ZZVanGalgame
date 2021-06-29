using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if(GameObject.Find("InputField").GetComponent<InputField>().text == "")
        {
            PlayerPrefs.SetString("PlayerName_Temp","你好像是个非常懒的屑因为你甚至懒得给自己起个名字");
        }
        else
        {
        PlayerPrefs.SetString("PlayerName_Temp",GameObject.Find("InputField").GetComponent<InputField>().text);
        Debug.Log(PlayerPrefs.GetString("PlayerName_Temp"));
        }
        SceneManager.LoadScene("1");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
