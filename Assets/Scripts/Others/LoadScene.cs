using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class LoadScene : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public Image timerFillImage;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public RectTransform timerTr;
    public RectTransform scoreTr;
    public Image[] fillStars;

    [SerializeField] private float lerpDuration;
    [SerializeField] private float disabledPos;
    private float ogPos;
    private void Awake()
    {
        GridUtils.onLevelMapLoad += OnMapLoaded;
        ogPos = timerTr.anchoredPosition.y;
        timerTr.anchoredPosition = new Vector2(timerTr.anchoredPosition.x, disabledPos);
        scoreTr.anchoredPosition = new Vector2(scoreTr.anchoredPosition.x, disabledPos);
    }
    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnMapLoaded;
    }

    private void OnMapLoaded(string path)
    {
        if (!TileSystem.Instance.isHub)
        {
            ActivateUI();
        }
    }

    public void ActivateUI()
    {
        DOTween.Kill(timerTr);
        DOTween.Kill(scoreTr);
        timerTr.DOAnchorPosY(ogPos, lerpDuration).SetUpdate(true).SetEase(TileSystem.Instance.easeOut);
        scoreTr.DOAnchorPosY(ogPos, lerpDuration).SetUpdate(true).SetEase(TileSystem.Instance.easeOut);
    }

    public void DisableUI()
    {
        DOTween.Kill(timerTr);
        DOTween.Kill(scoreTr);
        timerTr.DOAnchorPosY(disabledPos, lerpDuration).SetUpdate(true).SetEase(TileSystem.Instance.easeIn);
        scoreTr.DOAnchorPosY(disabledPos, lerpDuration).SetUpdate(true).SetEase(TileSystem.Instance.easeIn);
    }



    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void LoadSceneSingle(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void Update()
    {

        fpsText.text = (1 / Time.smoothDeltaTime).ToString();
    }
}
