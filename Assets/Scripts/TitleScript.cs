using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour
{
    public string SceneName;
    public void B_GameStart()
    {
        SceneManager.LoadScene(SceneName);
    }
}