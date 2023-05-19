using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public class CameraCtr : MonoBehaviour
{
    #region Variables
    #region Components and Lists
    private Camera cam;
    private CinemachineVirtualCamera dezoomCamera;
    private CinemachineTargetGroup targetGroup;
    private SplitScreenEffect sCE;
    [HideNormalInspector] public List<Transform> players;
    [HideNormalInspector] public List<ScreenData> sDatas;
    #endregion

    #region Main Variables
    public float distanceToSplit;
    public Color[] pCols;
    #endregion

    #region LineCast
    [Space(5)]
    [Header("WireFrame To Player")]
    public LayerMask lineCastLayers;
    public float sphereCastRadius;
    [Range(0,1)]public float transparencyLevel;
    #endregion

    #region ScreenShakes
    [Space(5)]
    [Header("Camera Shakes")]
    public float strongSS;
    public float mediumSS;
    public float weakSS;
    #endregion
    #endregion


    #region System CallBacks 
    private void Start()
    {
        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
        cam = Camera.main;
        SceneManager.sceneLoaded += OnLoad;
        sCE = GetComponentInChildren<SplitScreenEffect>();
        dezoomCamera = transform.GetChild(2).GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(changeCam());
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {

        TileSystem.Instance.StartCoroutine(OnLevelLoad());
        dezoomCamera.LookAt = TileSystem.Instance.centerTile.transform.GetChild(0).GetChild(0) ;
        for (int i = 0; i < sCE.Screens.Count; i++)
        {
            if (sCE.Screens[i].Target == null)
            {
                sCE.Screens.RemoveAt(i);
                i--;
            } 
        }
    }   

    private void Update()
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
    }

    private void LateUpdate()
    {
        LineCastToPlayer();
       
    }
    #endregion


    #region OnLoad
    IEnumerator changeCam()
    {
        yield return new WaitForSeconds(3);
        dezoomCamera.Priority = 2;
    }
    public IEnumerator OnLevelLoad()
    {
        yield return new WaitUntil(() => TileSystem.Instance.ready);
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
    public void AddPlayer(Transform player)
    {
        if(players == null)
        {
            players = new List<Transform>();
        }
        players.Add(player);
        SetPlayerColor(player);
        SetSplitScreens(player);
        SetTargetGroups();
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

    private void SetSplitScreens(Transform player)
    {
        if (sCE == null) sCE = GetComponentInChildren<SplitScreenEffect>(); ;
        if (sDatas.Count == 0)
        {
            sDatas = new List<ScreenData>();
            foreach (ScreenData data in sCE.Screens)
            {
                sDatas.Add(data);
            }
        }

        for (int i = 0; i < sCE.Screens.Count; i++)
        {
            sCE.Screens.RemoveAt(i);
            i--;
            if (sCE.Screens.Count == 0) break;
        }

        foreach (ScreenData screen in sDatas)
        {
            if (screen.Target != null) sCE.Screens.Add(screen);
            else if (screen.Target == null)
            {
                screen.Target = player;
                sCE.Screens.Add(screen);
                break;
            }
        }
    }

    private void SetTargetGroups()
    {
        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        for (int i = 0; i < players.Count; i++)
        {
            CinemachineTargetGroup.Target tar = new CinemachineTargetGroup.Target();
            tar.weight = 1;
            tar.target = players[i];
            targets.Add(tar);
        }
        if (targetGroup == null) targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            targetGroup.m_Targets[i] = targets[i];
        }
    }

    public void RemovePlayer(Transform player)
    {
        players.Remove(player);

        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        for (int i = 0; i < players.Count; i++)
        {
            CinemachineTargetGroup.Target tar = new CinemachineTargetGroup.Target();
            tar.weight = 1;
            tar.target = players[i];
            targets.Add(tar);
        }
        if (targetGroup == null) targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            targetGroup.m_Targets[i] = targets[i];
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
        Vector3 camPos = cam.transform.position;
        if (TileSystem.Instance.ready)
        {
            foreach(Transform player in  players)
            {
                Vector3 direction = (player.position - camPos).normalized;
                float distance = Vector3.Distance(player.position, cam.transform.position) - sphereCastRadius;
                RaycastHit[] hits = Physics.SphereCastAll(camPos, sphereCastRadius, direction, distance, lineCastLayers, QueryTriggerInteraction.Ignore);
                if (hits.Length > 0)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.transform.TryGetComponent<Tile>(out Tile tile))
                        {
                                tile.FadeTile(transparencyLevel);
                        }
                        else if (hit.transform.TryGetComponent<Interactor>(out Interactor inter))
                        {
                                inter.FadeTile(transparencyLevel);
                        }
                    }
                }
            }
        }
    }
    #endregion
}
