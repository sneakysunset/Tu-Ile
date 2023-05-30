using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;
using System.IO;
using UnityEngine.UI;
using DG.Tweening;
using NaughtyAttributes;

public class CameraCtr : MonoBehaviour
{
    #region Variables
    #region Components and Lists
    private Camera cam;
    [Foldout("Camera References")] public CinemachineVirtualCamera dezoomCamera;
    [Foldout("Camera References")] public CinemachineVirtualCamera cam1, cam2;
    [HideInInspector] public List<Transform> players;
    public delegate void OnStartUpDelegate();
    public static event OnStartUpDelegate startUp;
    [Foldout("Camera References")] public RectTransform SplitBar;
    float splitBarPosX;
    [Foldout("Camera References")] public Camera soloCam, duoCam2;
    #endregion

    #region Main Variables
    //public float distanceToSplit;
    public Color[] pCols;
    #endregion

    #region LineCast
    [Foldout("LineCast")] public LayerMask lineCastLayers;
    [Foldout("LineCast")] public float sphereCastRadius;
    [Foldout("LineCast")] [Range(0,1)] public float transparencyLevel;
    Tile[] fadeTile;
    Tile[] FadeTile {  get { return fadeTile; } set { if (fadeTile != value) OnFadeTileChange(value); } }
    #endregion

    #region ScreenShakes
    [Space(5)]
    [Header("Camera Shakes")]
    [Foldout("ScreenShakes")] public float strongSS;
    [Foldout("ScreenShakes")] public float mediumSS;
    [Foldout("ScreenShakes")] public float weakSS;
    #endregion

    #region ScreenShot
    [Foldout("ScreenShot")] public RenderTexture rtTarget;

    #endregion
    #endregion


    #region System CallBacks 
    private void Start()
    {
        if (startUp != null) startUp();
        splitBarPosX = SplitBar.anchoredPosition.x;
        cam = Camera.main;
        GridUtils.onLevelMapLoad += OnLoad;
        GridUtils.onEndLevel += OnEndLevel;
        //sCE = GetComponentInChildren<SplitScreenEffect>();
        dezoomCamera.LookAt = TileSystem.Instance.centerTile.minableItems;
        dezoomCamera.Follow = TileSystem.Instance.centerTile.minableItems;
        StartCoroutine(changeCam());
    }


    public void OnLoad()
    {
        StartCoroutine(OnLevelLoad());
        dezoomCamera.LookAt = TileSystem.Instance.centerTile.minableItems;
        dezoomCamera.Follow = TileSystem.Instance.centerTile.minableItems;
/*        for (int i = 0; i < sCE.Screens.Count; i++)
        {
            if (sCE.Screens[i].Target == null)
            {
                sCE.Screens.RemoveAt(i);
                i--;
            } 
        }*/
    }

    public void OnEndLevel(Tile tile) => DezoomCam(tile.minableItems);

/*    private void Update()
    {
        if(!TileSystem.Instance.isHub && players.Count > 1) 
        { 
            if (Vector3.Distance(players[0].transform.position, players[1].transform.position) > distanceToSplit)
            {
                sCE.enabled = true;
            }
            else if(Vector3.Distance(players[0].transform.position, players[1].transform.position) < distanceToSplit + 15)
            {
                sCE.enabled = false;   
            }        
        }
    }*/

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLoad;
    }

    private void LateUpdate()
    {
        LineCastToPlayer();
    }
    #endregion


    #region OnLoad
    IEnumerator changeCam()
    {
        yield return new WaitUntil(() => Input.GetButtonDown("StartGame"));
        dezoomCamera.Priority = 2;
        yield return new WaitForSeconds(GetComponentInChildren<CinemachineBrain>().m_DefaultBlend.m_Time);
        TileSystem.Instance.ready = true;
    }
    public IEnumerator OnLevelLoad()
    {
        TileSystem.Instance.ready = true;
        yield return new WaitForSeconds(2);
        //yield return new WaitUntil(() => TileSystem.Instance.ready);
        if (dezoomCamera != null)
        {
            dezoomCamera.Priority = 2;
        }
    }

    public void Dezoom(Transform targetTile)
    {

        dezoomCamera.Priority = 4;
    }

    public void DezoomCam(Transform targetTile)
    {
        if (dezoomCamera != null)
        {
            dezoomCamera.LookAt = targetTile;
            dezoomCamera.Priority = 4;
        }
    }
    #endregion


    #region AddPlayer
    public void AddPlayer(Transform player, Player playerP)
    {
        if(players == null)
        {
            players = new List<Transform>();
        }
        players.Add(player);
        playerP.playerIndex = players.Count - 1;
        if (players.Count > 1)
        {
            cam2.Follow = player ;
            cam2.LookAt = player;
        }
        else
        {
            cam1.Follow = player ;
            cam1.LookAt = player;
        }
        if(players.Count > 1) 
        { 
            Rect rect  = new Rect(0, 0, .5f, 1);
            soloCam.DORect(rect, 2).SetEase(TileSystem.Instance.easeInOut);
            duoCam2.gameObject.SetActive(true);
            SplitBar.transform.DOScaleX(1, 2).SetEase(TileSystem.Instance.easeInOut);
            SplitBar.DOAnchorPosX(0, 2).SetEase(TileSystem.Instance.easeInOut);
        }
        SetPlayerColor(player);
    }

    private void SetPlayerColor(Transform player)
    {
        SkinnedMeshRenderer[] sRs = player.parent.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (TileSystem.Instance.isHub)
        {
            switch (players.Count)
            {
                case 1: for (int j = 0; j < sRs.Length; j++) sRs[j].materials[1].color = pCols[0]; break;
                case 2: for (int j = 0; j < sRs.Length; j++) sRs[j].materials[1].color = pCols[1]; break;
                case 3: for (int j = 0; j < sRs.Length; j++) sRs[j].materials[1].color = pCols[2]; break;
                case 4: for (int j = 0; j < sRs.Length; j++) sRs[j].materials[1].color = pCols[3]; break;
            }
        }
    }

    public void RemovePlayer(Transform player)
    {
        players.Remove(player);
        if (players.Count == 1)
        {
            Rect rect = new Rect(0, 0, 1, 1);
            soloCam.DORect(rect , 2).SetEase(TileSystem.Instance.easeInOut);
            duoCam2.gameObject.SetActive(false);
            SplitBar.transform.DOScaleX(0, 2).SetEase(TileSystem.Instance.easeInOut);
            SplitBar.DOAnchorPosX(splitBarPosX, 2).SetEase(TileSystem.Instance.easeInOut);
        }
    }
    #endregion


    #region Camera Effects
    public void StartScreenShake(float duration, float magnitude)
    {
        ScreenShake.ScreenShakeEffect(duration, magnitude);
    }

    public void StartStrongScreenShake(float duration)
    {
        StartCoroutine(ScreenShake.ScreenShakeEffect(duration, strongSS));
    }

    public void StartMediumScreenShake(float duration)
    {
        StartCoroutine(ScreenShake.ScreenShakeEffect(duration, mediumSS));
    }

    public void StartWeakScreenShake(float duration)
    {
        StartCoroutine(ScreenShake.ScreenShakeEffect(duration, weakSS));
    }

    private void LineCastToPlayer()
    {
        Vector3 camPos = cam1.transform.position;
        if (TileSystem.Instance.ready)
        {
            List<Tile> tempList = new List<Tile>();
            foreach(Transform player in  players)
            {
                Vector3 direction = (player.position - camPos).normalized;
                float distance = Vector3.Distance(player.position, cam.transform.position) - sphereCastRadius;
                RaycastHit[] hits = Physics.SphereCastAll(camPos, sphereCastRadius, direction, distance, lineCastLayers, QueryTriggerInteraction.Ignore);
                if (hits.Length > 0) foreach (RaycastHit hit in hits) if (hit.transform.TryGetComponent<Tile>(out Tile tile) && !tempList.Contains(tile)) tempList.Add(tile);
/*                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.transform.TryGetComponent<Tile>(out Tile tile) && !tempList.Contains(tile))
                        {
                            tempList.Add(tile);
                        }
                    }
                }*/
            }
            FadeTile = tempList.ToArray();

            if(players.Count > 1)
            {

            }
/*            fadeTile = new Tile[tempList.Count];
            for (int i = 0; i < fadeTile.Length; i++)
            {
                fadeTile[i] = tempList[i];
            }*/
        }
    }

    private void OnFadeTileChange(Tile[] value)
    {
        if (fadeTile == null)
        {
            fadeTile = value;
            foreach (Tile tile in value) if (!tile.faded) tile.FadeTile(transparencyLevel);
            return;
        }
            
        foreach(Tile tile in fadeTile)
        {
            if(tile.faded) tile.UnFadeTile();
        } 
        foreach(Tile tile in value) if(!tile.faded) tile.FadeTile(transparencyLevel);

        fadeTile = value;
    }

    public void CamCapture()
    {
        Camera Cam = GetComponentInChildren<Camera>() ;
        Cam.targetTexture = rtTarget;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;

        Cam.Render();

        Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);
        string filePath = Application.streamingAssetsPath + "/ScreenShots/" + SceneManager.GetActiveScene().name;
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath); ;
        File.WriteAllBytes(Application.streamingAssetsPath + "/ScreenShots/" + SceneManager.GetActiveScene().name + "/SS_Game.png", Bytes);
        Cam.targetTexture = null;

    }

    public void RenderTextureOnImage(RawImage image, string sceneName)
    {
        string filename = Application.streamingAssetsPath + "/ScreenShots/" + sceneName + "/SS_Game.png";
        if (!File.Exists(filename)) return;
        var rawData = System.IO.File.ReadAllBytes(filename);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(rawData);
        image.texture = tex;
    }
    #endregion
}
