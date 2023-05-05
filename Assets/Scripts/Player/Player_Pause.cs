using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Pause : MonoBehaviour
{
    public bool isPaused;
    PlayerInput[] players;
    [HideInInspector] public PauseMenu pauseMenu;

    private void Start()
    {
        players = FindObjectsOfType<PlayerInput>();

    }

    public void PauseGame()
    {
        if (isPaused)
        {
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
