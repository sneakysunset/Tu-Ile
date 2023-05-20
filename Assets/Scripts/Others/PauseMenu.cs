using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public Player_Pause player;
    public AnimationCurve easeIn;
    public AnimationCurve easeOut;
    private PlayerInput[] players;
    private RectTransform tr;
    private PlayerInputManager playerInputManager;
    private void Awake()
    {
        players = FindObjectsOfType<PlayerInput>();
        tr = transform.GetChild(0) as RectTransform;
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    public void EnablePause(Player_Pause _player)
    {
        player = _player;
        playerInputManager.enabled = false;
        Time.timeScale = 0;
        tr.DOAnchorPosX(0, 1,true).SetEase(easeOut).SetUpdate(true);
        foreach (PlayerInput p in players)
        {
            if (p.transform != transform)
            {
                p.enabled = false;
            }
        }
    }

    public void DisablePause()
    {
        playerInputManager.enabled = true;
        Time.timeScale = 1;
        tr.DOAnchorPosX(465, 1, true).SetEase(easeOut).SetUpdate(false);

        foreach (PlayerInput p in players)
        {
            if (p.transform != transform)
            {
                p.enabled = true;
            }
        }
    }

    public void Resume()
    {
        if(player) player.SetPause();
    }

    public void Options()
    {

    }

    public void Restart()
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        gameTimer.sceneLoadName = SceneManager.GetActiveScene().name;
        gameTimer.EndLevel(false);

        player.SetPause();
    }

    public void HUB()
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        gameTimer.sceneLoadName = "Hub";
        gameTimer.EndLevel(false);
        player.SetPause();
    }
}
