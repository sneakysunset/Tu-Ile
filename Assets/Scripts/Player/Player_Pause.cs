using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player_Pause : MonoBehaviour
{
    public bool isPaused;
    PlayerInput[] players;
    [HideInInspector] public PauseMenu pauseMenu;
    public delegate void PauseMenuDelegate();
    public static event PauseMenuDelegate pauseMenuActivation;
    public delegate void PauseMenuDesDelegate();
    public static event PauseMenuDesDelegate pauseMenuDesactivation;
    private void Start()
    {
        players = FindObjectsOfType<PlayerInput>();
        SceneManager.sceneLoaded += OnLoaded;
    }

    private void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pauseMenu == null) pauseMenu = FindObjectOfType<PauseMenu>();
    }

    public void PauseGame()
    {
        if(!TileSystem.Instance.isHub)
        {
            if (isPaused)
            {
                if (pauseMenuDesactivation != null) pauseMenuDesactivation();
                isPaused = false;
                Time.timeScale = 1.0f;
                pauseMenu.gameObject.SetActive(false);
                pauseMenu.player = null;
                foreach(PlayerInput p in players)
                {
                    if(p.transform != transform)
                    {
                        p.enabled = true;
                    }
                }
            }
            else
            {
                if (pauseMenuActivation != null) pauseMenuActivation();
                isPaused = true;
                Time.timeScale = 0;
                pauseMenu.gameObject.SetActive(true);
                pauseMenu.player = this;
                foreach (PlayerInput p in players)
                {
                    if (p.transform != transform)
                    {
                        p.enabled = false;
                    }
                }
            }
        }
    }
}
