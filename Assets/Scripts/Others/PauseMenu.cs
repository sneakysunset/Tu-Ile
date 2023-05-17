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
        StartCoroutine(TileSystem.Instance.SinkWorld(TileSystem.Instance.centerTile, SceneManager.GetActiveScene().name));
    }

    public void HUB()
    {
        //StartCoroutine(TileSystem.Instance.SinkWorld("Hub"));
        FindObjectOfType<GameTimer>().timer = FindObjectOfType<GameTimer>().gameTimer;
        player.PauseGame();
    }
}
