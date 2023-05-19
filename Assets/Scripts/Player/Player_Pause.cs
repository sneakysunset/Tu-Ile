using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player_Pause : MonoBehaviour
{
    public bool isPaused;
    [HideInInspector] public PauseMenu pauseMenu;
    public delegate void PauseMenuDelegate();
    public static event PauseMenuDelegate pauseMenuActivation;
    public delegate void PauseMenuDesDelegate();
    public static event PauseMenuDesDelegate pauseMenuDesactivation;
    private void Start()
    {
        SceneManager.sceneLoaded += OnLoaded;
    }

    private void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pauseMenu == null) pauseMenu = FindObjectOfType<PauseMenu>();
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if(!TileSystem.Instance.isHub && context.started)
        {
            SetPause();
        }
    }

    public void SetPause()
    {
        if (isPaused)
        {
            if (pauseMenuDesactivation != null) pauseMenuDesactivation();
            isPaused = false;
            pauseMenu.DisablePause();
        }
        else
        {
            if (pauseMenuActivation != null) pauseMenuActivation();
            isPaused = true;
            pauseMenu.EnablePause(this);
        }
    }
}
