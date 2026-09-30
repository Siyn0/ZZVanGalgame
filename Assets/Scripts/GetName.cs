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
        var target = GameObject.Find("CText");
        NameText = target ? target.GetComponent<Text>() : null;
        if (NameText) NameText.text = ZZVan.Galgame.GameSession.Instance.State.playerName;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
