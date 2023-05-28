using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public Player_Pause player;
    public AnimationCurve easeIn;
    public AnimationCurve easeOut;
    private PlayerInput[] players;
    private RectTransform tr;
    private PlayerInputManager playerInputManager;
    public Button ogButton;
    public Button optionButton;
    public bool optionOn;

    private void Awake()
    {
        players = FindObjectsOfType<PlayerInput>();
        tr = transform.GetChild(0) as RectTransform;
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    public void EnablePause(Player_Pause _player)
    {
        ogButton.Select();
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
        EventSystem.current.SetSelectedGameObject(null);

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
        if (optionOn) return;
        if (player) player.SetPause();
    }

    public void Options()
    {
        if(optionOn) return;
        optionOn = true;
    }

    public void Restart()
    {
        if (optionOn) return;

        TileSystem.Instance.gameTimer.EndLevel(false, false);

        player.SetPause();
    }

    public void HUB()
    {
        if (optionOn) return;
        TileSystem.Instance.gameTimer.EndLevel(false, true);
        player.SetPause();
    }
}
