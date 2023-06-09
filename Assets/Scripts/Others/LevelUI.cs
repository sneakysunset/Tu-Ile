using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LevelUI : MonoBehaviour
{
    [HideNormalInspector] public Transform mainCamera;
    public GameObject NearUI;
    public GameObject DetailUI;
    public GameObject NoDetailUI;
    public RawImage levelIcon;
    public Image[] Stars;
    [SerializeField] private GameObject UIRoot;
    private RessourcesManager rMan;
    [SerializeField] private RectTransform detailRight, detailLeft;
    [SerializeField] private TextMeshProUGUI timeT, scoreT;
    [SerializeField] private TextMeshProUGUI[] scoreCapValue;
    Tile tile;
    private ScoreManager scoreManager;
    private GameTimer gameTimer;

    private void OnEnable()
    {
        GridUtils.onLevelMapLoad += OnLevelLoad;
        GridUtils.onStartLevel += OnStartLevel;
    }

    public void EnableUI() => UIRoot.SetActive(true);

    public void DisableUI() => UIRoot.SetActive(false);

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
        RessourcesManager rMan = RessourcesManager.Instance;
        gameTimer = rMan.getGameManagerFromList(tile.levelName + "_GM");
        scoreManager = gameTimer.GetComponent<ScoreManager>();
        StarActivation(string.Empty);
        timeT.text = gameTimer.gameTimer.ToString();
    }

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLevelLoad;
        GridUtils.onStartLevel -= OnStartLevel;
    }

    void OnLevelLoad(string path)
    {
        if(tile.tileType == TileType.LevelLoader && TileSystem.Instance.isHub)
        {
            StarActivation(path);
            OnActivated();
        }
    }

    private void StarActivation(string path)
    {
        scoreT.text = scoreManager.highscore.ToString();
        int[] scoreCaps = scoreManager.scoreCaps;
        int highScore = scoreManager.highscore;

        scoreCapValue[0].text = scoreManager.scoreCaps[0].ToString();
        scoreCapValue[1].text = scoreManager.scoreCaps[1].ToString();
        scoreCapValue[2].text = scoreManager.scoreCaps[2].ToString();
        for (int i = 0; i < Stars.Length; i++)
        {
            if (highScore >= scoreCaps[i])
            {
                Stars[i].color = Color.yellow;
                scoreCapValue[i].color = Color.black;
            }
            else
            {
                Stars[i].color = Color.black;
                scoreCapValue[i].color = Color.yellow;
            }
        }
    }

    void OnStartLevel()
    {
        if(scoreManager.isCompleted && !scoreManager.activated)
        {
            scoreManager.activated = true;
            TileSystem.Instance.tutorial.GetComponent<HubEvents>().GrowTileList();
        }
    }

    public void OnActivated()
    {
        if (Camera.main) mainCamera = Camera.main.transform;
       
        TileSystem.Instance.cam.RenderTextureOnImage(levelIcon, tile.levelName);
    }

    void Update()
    {
        while(mainCamera == null) OnActivated();
        //transform.position = cam.targetGroup.transform.position;
        if (mainCamera) NearUI.transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }

    public void PlayerNear()
    {
        NearUI.gameObject.SetActive(true);
    }

    public void PlayerFar()
    {
        NearUI.gameObject.SetActive(false);
    }

    public void NoDetail()
    {
        //DetailUI.gameObject.SetActive(false);
        //NoDetailUI.gameObject.SetActive(true);
        detailRight.DOAnchorPosX(0, 1).SetEase(TileSystem.Instance.easeInOut);
        detailLeft.DOAnchorPosX(0, 1).SetEase(TileSystem.Instance.easeInOut);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Paper");
    }

    public void Detail()
    {
        //DetailUI.gameObject.SetActive(true);
        detailLeft.DOAnchorPosX(-1.8f, 1).SetEase(TileSystem.Instance.easeInOut);
        detailRight.DOAnchorPosX(1.8f, 1).SetEase(TileSystem.Instance.easeInOut);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Paper");
        // NoDetailUI.gameObject.SetActive(false);
    }
}
