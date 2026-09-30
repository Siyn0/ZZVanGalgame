using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GetName : MonoBehaviour
{

    private Text NameText;
    // Start is called before the first frame update
    void Start()
    {
        NameText = GameObject.Find("CText").GetComponent<Text>();
        NameText.text = PlayerPrefs.GetString("PlayerName_Temp");
        Debug.Log(NameText.text);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
