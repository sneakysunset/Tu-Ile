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
    TextMeshProUGUI timeT, scoreT;
    Tile tile;


    private void Awake()
    {
        GridUtils.onLevelMapLoad += OnLevelLoad;
    }

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLevelLoad;
    }

    void OnLevelLoad(string path)
    {
        if(tile.tileType == TileType.LevelLoader && TileSystem.Instance.isHub) StarActivation(path);
    }

    private void StarActivation(string path)
    {
        RessourcesManager rMan = RessourcesManager.Instance;
        ScoreManager scoreMan = rMan.getGameManagerFromList(tile.levelName + "_GM").scoreMan;
        int[] scoreCaps = scoreMan.scoreCaps;
        int highScore = scoreMan.highscore;
        for (int i = 0; i < Stars.Length; i++)
        {
            if (highScore > scoreCaps[i]) Stars[i].enabled = true;
            else Stars[i].enabled = false;
        }
    }

    public void OnActivated()
    {
        tile = GetComponentInParent<Tile>();
        rMan = RessourcesManager.Instance;
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
        detailLeft.DOAnchorPosX(-1.5f, 1).SetEase(TileSystem.Instance.easeInOut);
        detailRight.DOAnchorPosX(1.5f, 1).SetEase(TileSystem.Instance.easeInOut);
       // NoDetailUI.gameObject.SetActive(false);
    }
}
