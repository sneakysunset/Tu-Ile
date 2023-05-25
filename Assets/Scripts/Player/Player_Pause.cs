using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player_Pause : MonoBehaviour
{
    public bool isPaused;
    [HideInInspector] public PauseMenu pauseMenu;
    [HideInInspector] public OptionMenu optionMenu;
    public delegate void PauseMenuDelegate();
    public static event PauseMenuDelegate pauseMenuActivation;
    public delegate void PauseMenuDesDelegate();
    public static event PauseMenuDesDelegate pauseMenuDesactivation;
    private void Start()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        optionMenu = FindObjectOfType<OptionMenu>();
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if(!TileSystem.Instance.isHub && context.started && TileSystem.Instance.ready)
        {
            SetPause();
        }
    }

    public void SetPause()
    {
        if (pauseMenu.optionOn)
        {
            optionMenu.ExitOptions();
            optionMenu.transform.GetChild(0).gameObject.SetActive(false);
            pauseMenu.optionOn = false;
        }

        else if (isPaused)
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
