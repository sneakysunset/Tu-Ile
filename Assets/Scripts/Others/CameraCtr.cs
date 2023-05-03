using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtr : MonoBehaviour
{
    public float smoother;
    private Vector3 velocity;
    private Camera cam;
    private List<Transform> players;
    public LayerMask lineCastLayers;
    public float sphereCastRadius;
    [Range(0,1)]public float transparencyLevel;
    public Vector3 medianPos;
    public CinemachineVirtualCamera virtualCamera, endVirtualCam;
    private Vector3 direction;
    private float distance;
    private SplitScreenEffect sCE;
    public float distanceToSplit;
    
    IEnumerator Start()
    {
        cam = Camera.main;
        direction = cam.transform.position - transform.position;
        sCE = GetComponentInChildren<SplitScreenEffect>();
        sCE.enabled = false;
        yield return new WaitUntil(()=> Input.GetKeyDown(KeyCode.Space));
        if(virtualCamera != null)
        {
            virtualCamera.Priority = 2;
        }
    }

    private void Update()
    {
        if(players.Count > 1) 
        { 
            if (Vector3.Distance(players[0].transform.position, players[1].transform.position) > distanceToSplit)
            {
                sCE.enabled = true;
            }
            else
            {
                sCE.enabled = false;
            }        
        }
    }

    public void Dezoom()
    {
        if (virtualCamera != null)
        {
            endVirtualCam.Priority = 4;
        }
    }
    private void LateUpdate()
    {
        LineCastToPlayer();
        Vector3 medianPos = Vector3.zero;
        foreach (Transform player in players)
        {
            medianPos += player.transform.position;
        }
        medianPos /= players.Count;
        //transform.position = Vector3.SmoothDamp(transform.position, medianPos, ref velocity, smoother);
    }

    public void AddPlayer(Transform player)
    {
        if(players == null)
        {
            players = new List<Transform>();
        }
        players.Add(player);
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
