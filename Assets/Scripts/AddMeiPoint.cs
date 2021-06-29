using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AddMeiPoint : MonoBehaviour
{
    public int MeiAdd;
    public string SceneName;
    
    private int MeiTemp;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
        //MeiTemp = 0.MeiPoint;
    }

    void OnClick()
    {
        MeiTemp = PlayerPrefs.GetInt("MeiMei_Temp");
        MeiTemp += MeiAdd;
        PlayerPrefs.SetInt("MeiMei_Temp",MeiTemp);
        SceneManager.LoadScene(SceneName);
        Debug.Log("妹妹好感+" + MeiAdd.ToString());
        Debug.Log("妹妹好感:" + PlayerPrefs.GetInt("MeiMei_Temp"));
    }
    // Update is called once per frame
    void Update()
    {

    }
}
