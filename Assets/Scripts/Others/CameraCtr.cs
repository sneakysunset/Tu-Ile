using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtr : MonoBehaviour
{
    public float smoother;
    private Vector3 velocity;
    private Camera cam;
    private Transform player;
    public LayerMask lineCastLayers;
    public float sphereCastRadius;
    void Start()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = player.position;
    }

    private void Update()
    {
        LineCastToPlayer();

    }

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position, ref velocity, smoother);
    }

    private void LineCastToPlayer()
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
                        hit.transform.gameObject.layer = 7;
                    }
                    tile.FadingTileEffect();
                }
            }
        }
    }
}
