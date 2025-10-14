using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //public LoadMap LoadMap;
    [Header("References")]
    public MovePlayer movePlayer;
    public LoadMap loadMap;
    public HealthSystem playerHealthSystem;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public GameObject pauseButton;
    public GameObject continueButton;
    public GameObject QuitButton;

    [Header("Restart Button")]
    public GameObject restartButton;

    [Header("Win Game")]
    public GameObject winScreen;


    private void Start()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(false);
        continueButton.SetActive(false);
        QuitButton.SetActive(false);
        winScreen.SetActive(false);
    }

    private void Update()
    {
        CheckIfPaused();
    }

    // ---------- pause menu ----------
    void CheckIfPaused()
    {
        if (Input.GetKeyDown("escape"))
        {
            Debug.Log("Pause button pressed");
            pausePanel.SetActive(true);
            pauseButton.SetActive(true);
            continueButton.SetActive(true);
            QuitButton.SetActive(true);

            Time.timeScale = 0;

        }
    }
    public void Continue()
    {
        Debug.Log("Continue button pressed");
        pausePanel.SetActive(false);
        pauseButton.SetActive(false);
        continueButton.SetActive(false);
        QuitButton.SetActive(false);

        Time.timeScale = 1; // unpause
    }
    public void QuitToMenu()
    {
        Debug.Log("Quit button pressed");
        SceneManager.LoadScene(0); // load MainMenu
    }

    // ---------- resets map so player can restart level ----------
    public void Restart() 
    {
        Debug.Log("Restart button pressed");
        movePlayer.ResetPosition(); 
    }
    public void NextNevel() 
    {
        Debug.Log("Loading next map...");
        movePlayer.ResetPosition();
        playerHealthSystem.ResetHealth();
        loadMap.LoadPremadeMap();
    }

    public void WinGame()
    {
        Debug.Log("Win game");
        winScreen.SetActive(true);
        Time.timeScale = 0;
    }
}