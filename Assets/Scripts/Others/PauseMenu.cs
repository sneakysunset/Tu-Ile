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
    [SerializeField] private LoadScene canvasRef;
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
        if (!TileSystem.Instance.isHub)
        {
            canvasRef.DisableUI();
        }
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
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");

        if (optionOn) return;
        if (!TileSystem.Instance.isHub)
        {
            canvasRef.ActivateUI();
        }
        if (player && !TileSystem.Instance.isHub) player.SetPause();
        else if (player && TileSystem.Instance.isHub) player.SetHubPause();
    }

    public void Options()
    {

        if (optionOn) return;
        optionOn = true;
    }

    public void Restart()
    {
        if (optionOn) return;


        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");
        TileSystem.Instance.gameTimer.EndLevel(false, false);

        player.SetPause();
    }

    public void HUB()
    {
        if (optionOn) return;
        if (TileSystem.Instance.isHub)
        {
            Application.Quit();
        }
        else
        {
            TileSystem.Instance.gameTimer.EndLevel(false, true);
            player.SetPause();
        }
    }
}
