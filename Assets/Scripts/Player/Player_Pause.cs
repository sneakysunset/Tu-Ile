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
    [HideInInspector] public PauseMenu pauseHubMenu;
    public delegate void PauseMenuDelegate(Player player);
    public static event PauseMenuDelegate pauseMenuActivation;
    public delegate void PauseMenuDesDelegate(Player player);
    public static event PauseMenuDesDelegate pauseMenuDesactivation;
    private Player player;
    private void Start()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        optionMenu = FindObjectOfType<OptionMenu>();
        player = GetComponent<Player>();
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if(!TileSystem.Instance.isHub && context.started && TileSystem.Instance.ready)
        {
            SetPause();
        }
        else if(TileSystem.Instance.isHub && context.started && TileSystem.Instance.ready)
        {
            SetHubPause();
        }
    }

    public void SetPause()
    {
        if (pauseHubMenu.optionOn)
        {
            optionMenu.ExitOptions();
            optionMenu.transform.GetChild(0).gameObject.SetActive(false);
            pauseMenu.optionOn = false;
        }
        else if (isPaused)
        {
            if (pauseMenuDesactivation != null) pauseMenuDesactivation(player);
            isPaused = false;
            pauseMenu.DisablePause();
        }
        else
        {
            if (pauseMenuActivation != null) pauseMenuActivation(player);
            isPaused = true;
            pauseMenu.EnablePause(this);
        }
    }

    public void SetHubPause()
    {
        if (pauseMenu.optionOn)
        {
            optionMenu.ExitOptions();
            optionMenu.transform.GetChild(0).gameObject.SetActive(false);
            pauseHubMenu.optionOn = false;
        }

        else if (isPaused)
        {
            if (pauseMenuDesactivation != null) pauseMenuDesactivation(player);
            isPaused = false;
            pauseHubMenu.DisablePause();
        }
        else
        {
            if (pauseMenuActivation != null) pauseMenuActivation(player);
            isPaused = true;
            pauseHubMenu.EnablePause(this);
        }
    }
}
