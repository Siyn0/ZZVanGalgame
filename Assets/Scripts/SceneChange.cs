using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    public string SceneName;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void onClick()
    {
        SceneManager.LoadScene(SceneName);
    }



    // Update is called once per frame
    void Update()
    {

    }
}
