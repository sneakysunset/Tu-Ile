using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtr : MonoBehaviour
{
    public float smoother;
    private Vector3 velocity;
    private Camera cam;
    private GameObject[] players;
    public LayerMask lineCastLayers;
    public float sphereCastRadius;
    void Start()
    {
        cam = Camera.main;
        players = GameObject.FindGameObjectsWithTag("Player");
        Vector3 medianPos = Vector3.zero;
        foreach(GameObject player in players)
        {
            medianPos += player.transform.position;
        }
        medianPos /= players.Length;
        transform.position = medianPos;
    }

    private void Update()
    {
        //LineCastToPlayer();

    }

    private void LateUpdate()
    {
        Vector3 medianPos = Vector3.zero;
        foreach (GameObject player in players)
        {
            medianPos += player.transform.position;
        }
        medianPos /= players.Length;
        transform.position = Vector3.SmoothDamp(transform.position, medianPos, ref velocity, smoother);
    }

/*    private void LineCastToPlayer()
    {
        Vector3 camPos = cam.transform.position;
        Vector3 direction = (player.position - camPos).normalized;
        float distance = Vector3.Distance(player.position, cam.transform.position) - sphereCastRadius;
        RaycastHit[] hits = Physics.SphereCastAll(camPos,sphereCastRadius, direction, distance, lineCastLayers, QueryTriggerInteraction.Ignore);
        if(hits.Length > 0)
        {
            foreach(RaycastHit hit in hits)
            {
                if(hit.transform.gameObject.layer == 6)
                {
                    Tile tile = hit.transform.GetComponent<Tile>();
                    if (!tile.isSelected)
                    {
                        //hit.transform.gameObject.layer = 7;
                    }
                    tile.FadingTileEffect();
                }
            }
        }
    }*/
}
