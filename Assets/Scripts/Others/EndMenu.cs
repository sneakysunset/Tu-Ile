using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Numerics;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EndMenu : MonoBehaviour
{
    public AnimationCurve easeIn, easeOut;
    private PlayerInput[] players;
    private RectTransform tr;
    public TextMeshProUGUI missionsReussies, missionsRatees, total;
    private PauseMenu pauseMenu;
    private RawImage screenShot;
    public Button ogButton;
    public Button optionButton;
    public delegate void OnLevelEnd();
    public OnLevelEnd onLevelEnd;
    private void Start()
    {
        players = FindObjectsOfType<PlayerInput>();
        pauseMenu = FindObjectOfType<PauseMenu>();  
        tr = transform.GetChild(0) as RectTransform;
        screenShot = GetComponentInChildren<RawImage>();

    }

    public IEnumerator EnableEnd()
    {
        if(onLevelEnd != null) onLevelEnd();
        GameTimer gameTimer = TileSystem.Instance.gameTimer;
        //MissionManager missionManager = MissionManager.Instance;
        CameraCtr cam = TileSystem.Instance.cam;

        //StartCoroutine(gameTimer.LerpTimeLine(timerTr.anchoredPosition, timerTr.anchoredPosition + UnityEngine.Vector2.up * -100, timerTr, gameTimer.lerpCurveEaseIn, gameTimer.lerpSpeed));
        cam.DezoomCam(TileSystem.Instance.centerTile.tc.minableItems);
        //missionManager.CloseMissions();
        gameTimer.canvasRef.DisableUI();
        yield return new WaitForSeconds(2);

        cam.CamCapture();
        cam.RenderTextureOnImage(screenShot, SceneManager.GetActiveScene().name);
        //missionsReussies.text = missionManager.numberOfClearedMissions.ToString();
        //missionsRatees.text = missionManager.numberOfFailedMissions.ToString();
        total.text = TileSystem.Instance.scoreManager.score.ToString();
        TileSystem.Instance.playersMan.enabled = false;
        Time.timeScale = 0;
        tr.DOAnchorPosY(0, 1, true).SetEase(easeOut).SetUpdate(true);
        ogButton.Select();
        foreach (PlayerInput p in players)
        {
            if (p.transform != transform)
            {
                p.enabled = false;
            }
        }
    }

    public void DisableEnd()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");
        EventSystem.current.SetSelectedGameObject(null);

        TileSystem.Instance.playersMan.enabled = true;
        Time.timeScale = 1;
        tr.DOAnchorPosY(1130, 1, true).SetEase(easeOut).SetUpdate(true);

        foreach (PlayerInput p in players)
        {
            if (p.transform != transform)
            {
                p.enabled = true;
            }
        }
    }

    public void Options()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");

        EventSystem.current.SetSelectedGameObject(null);

        if (pauseMenu.optionOn) return;
        pauseMenu.optionOn = true;
    }

    public void Restart()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");

        DisableEnd();
        //TileSystem.Instance.gameTimer.sceneLoadName = SceneManager.GetActiveScene().name;
        TileSystem.Instance.gameTimer.EndLevel(true, false);
    }

    public void HUB()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button");

        DisableEnd();
        //TileSystem.Instance.gameTimer.sceneLoadName = "Hub";
        TileSystem.Instance.gameTimer.EndLevel(true, true);
    }
}
