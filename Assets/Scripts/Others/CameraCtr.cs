using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public class CameraCtr : MonoBehaviour
{
    public float smoother;
    private Vector3 velocity;
    private Camera cam;
    [HideNormalInspector] public List<Transform> players;
    public LayerMask lineCastLayers;
    public float sphereCastRadius;
    [Range(0,1)]public float transparencyLevel;
    public Vector3 medianPos;
    public CinemachineVirtualCamera dezoomCamera;
    private Vector3 direction;
    private float distance;
    private SplitScreenEffect sCE;
    public float distanceToSplit;
    private CinemachineTargetGroup targetGroup;
    private List<ScreenData> tempScreens;

    public Material player1Mat, player2Mat, player3Mat, player4Mat;
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        cam = Camera.main;
        direction = cam.transform.position - transform.position;
        SceneManager.sceneLoaded += OnLoad;
        sCE = GetComponentInChildren<SplitScreenEffect>();
        StartCoroutine(changeCam());
        tempScreens = sCE.Screens;
    }


    IEnumerator changeCam()
    {
        yield return new WaitForSeconds(1);
        dezoomCamera.Priority = 2;
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

    public IEnumerator OnLevelLoad()
    {
        yield return new WaitUntil(() => TileSystem.Instance.ready);
        if (dezoomCamera != null)
        {
            dezoomCamera.Priority = 2;
        }
    }

    public void Dezoom()
    {
        dezoomCamera.Priority = 4;
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

    public void DezoomCam()
    {
        if (dezoomCamera != null)
        {
            dezoomCamera.Priority = 4;
        }
    }
    private void LateUpdate()
    {
        LineCastToPlayer();
       
    }

    public void AddPlayer(Transform player)
    {
        if(players == null)
        {
            players = new List<Transform>();
        }
        players.Add(player);
        SkinnedMeshRenderer[] sRs = player.parent.GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshRenderer meshRenderer = player.parent.GetComponentInChildren<MeshRenderer>();

        foreach(ScreenData screen in sCE.Screens)
        {
            if(screen.Target == null)
            {
                screen.Target = player;
                break;
            }
        }

        switch (players.Count)
        {
            case 1: for (int j = 0; j < sRs.Length; j++) sRs[j].material = player1Mat; meshRenderer.material = player1Mat; break;
            case 2: for (int j = 0; j < sRs.Length; j++) sRs[j].material = player2Mat; meshRenderer.material = player2Mat; break;
            case 3: for (int j = 0; j < sRs.Length; j++) sRs[j].material = player3Mat; meshRenderer.material = player3Mat; break;
            case 4: for (int j = 0; j < sRs.Length; j++) sRs[j].material = player4Mat; meshRenderer.material = player4Mat; break;
        }

        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        for (int i = 0; i < players.Count; i++)
        {
            CinemachineTargetGroup.Target tar = new CinemachineTargetGroup.Target();
            tar.weight = 1;
            tar.target = players[i];
            targets.Add(tar);
        }
        if(targetGroup == null) targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
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

    public void StartScreenShake(float duration, float magnitude)
    {
        ScreenShake.ScreenShakeEffect(duration, magnitude);
    }

    public float strongSS;
    public float mediumSS;
    public float weakSS;
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
}
