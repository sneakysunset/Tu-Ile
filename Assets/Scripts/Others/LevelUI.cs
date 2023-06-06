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
    private RessourcesManager rMan;
    [SerializeField] private RectTransform detailRight, detailLeft;
    [SerializeField] private TextMeshProUGUI timeT, scoreT;
    [SerializeField] private TextMeshProUGUI[] scoreCapValue;
    Tile tile;
    private ScoreManager scoreManager;
    private GameTimer gameTimer;

    private void Enable()
    {
        GridUtils.onLevelMapLoad += OnLevelLoad;
    }

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
    }

    void OnLevelLoad(string path)
    {
        print(3);
        if(tile.tileType == TileType.LevelLoader && TileSystem.Instance.isHub) StarActivation(path);
    }

    private void StarActivation(string path)
    {
        scoreT.text = scoreManager.highscore.ToString();
        print(scoreManager.highscore);
        int[] scoreCaps = scoreManager.scoreCaps;
        int highScore = scoreManager.highscore;
        scoreCapValue[0].text = scoreManager.scoreCaps[0].ToString();
        scoreCapValue[1].text = scoreManager.scoreCaps[1].ToString();
        scoreCapValue[2].text = scoreManager.scoreCaps[2].ToString();
        for (int i = 0; i < Stars.Length; i++)
        {
            print(highScore + " " + scoreCaps[i]);
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

    public void OnActivated()
    {
        if (Camera.main) mainCamera = Camera.main.transform;
       
        TileSystem.Instance.cam.RenderTextureOnImage(levelIcon, tile.levelName);
    }

    void Update()
    {
        while(mainCamera == null) OnActivated();
        //transform.position = cam.targetGroup.transform.position;
        if (mainCamera) transform.GetChild(0).LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
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
    }

    public void Detail()
    {
        //DetailUI.gameObject.SetActive(true);
        detailLeft.DOAnchorPosX(-2f, 1).SetEase(TileSystem.Instance.easeInOut);
        detailRight.DOAnchorPosX(2f, 1).SetEase(TileSystem.Instance.easeInOut);
       // NoDetailUI.gameObject.SetActive(false);
    }
}
