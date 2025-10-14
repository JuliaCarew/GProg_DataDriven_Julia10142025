using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_SceneLoader : MonoBehaviour
{
    // ---------- Main Menu ----------
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    public void StartGame()
    {
        Debug.Log("StartGame");
        SceneManager.LoadScene(1);
    }
}
