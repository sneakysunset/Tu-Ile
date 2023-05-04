using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public Player_Pause player;

    public void Resume()
    {
        player.PauseGame();
    }

    public void Options()
    {

    }

    public void HUB()
    {
        SceneManager.LoadScene("Hub");
    }
}
