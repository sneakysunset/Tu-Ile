using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Numerics;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class EndMenu : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public AnimationCurve easeIn, easeOut;
    private PlayerInput[] players;
    private RectTransform tr;
    public TextMeshProUGUI missionsReussies, missionsRatees, total;
    private GameTimer gameTimer;

    private void Start()
    {
        players = FindObjectsOfType<PlayerInput>();
        tr = transform.GetChild(0) as RectTransform;
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        gameTimer = FindObjectOfType<GameTimer>();
    }

    public IEnumerator EnableEnd()
    {
        MissionManager missionManager = MissionManager.Instance;
        
        StartCoroutine(gameTimer.LerpTimeLine(gameTimer.UIPos.anchoredPosition, gameTimer.UIPos.anchoredPosition + UnityEngine.Vector2.up * -100, gameTimer.UIPos, gameTimer.lerpCurveEaseIn, gameTimer.lerpSpeed));
        FindObjectOfType<CameraCtr>().DezoomCam(TileSystem.Instance.centerTile.transform.GetChild(0));
        missionManager.CloseMissions();

        yield return new WaitForSeconds(2);

        missionsReussies.text = missionManager.numberOfClearedMissions.ToString();
        missionsRatees.text = missionManager.numberOfFailedMissions.ToString();
        total.text = ScoreManager.Instance.score.ToString();
        playerInputManager.enabled = false;
        Time.timeScale = 0;
        tr.DOAnchorPosY(0, 1, true).SetEase(easeOut).SetUpdate(true);

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
        playerInputManager.enabled = true;
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

    }

    public void Restart()
    {
        DisableEnd();
        gameTimer.sceneLoadName = SceneManager.GetActiveScene().name;
        gameTimer.EndLevel(true);
    }

    public void HUB()
    {
        DisableEnd();
        gameTimer.sceneLoadName = "Hub";
        gameTimer.EndLevel(true);
    }
}
