using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    GameObject spawnedItem;
    public GameObject itemToSpawn;
    private Transform spawnPoint;
    public float spawnTimer = 3f;
    private float baseTimerValue;
    public bool loop;
    bool continueLooping = true;
    Tile tile;
    [Foldout("Gizmo")] public float gizmoScale = 1;
    [Foldout("Gizmo")] public float gizmoHeightOffset = 0;
    private void Start()
    {
        tile = GetComponent<Tile>();
        spawnPoint = transform.Find("SpawnPositions");
        baseTimerValue = spawnTimer;
    }

    private void SpawnItem()
    {
        spawnedItem = Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);
        if(!loop) { continueLooping = false; }
    }

    private void Update()
    {
        if(TileSystem.Instance.ready && spawnedItem == null && tile.walkable && continueLooping)
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer < 0)
            {
                spawnTimer = baseTimerValue;
                SpawnItem();
            }
        }
    }

    Mesh meshGizmo;
    private void OnDrawGizmos()
    {
        if(meshGizmo == null || spawnPoint == null)
        {
            meshGizmo = itemToSpawn.transform.Find("Highlight").GetComponent<MeshFilter>().sharedMesh;
            spawnPoint = transform.Find("SpawnPositions");
        }
        Gizmos.DrawMesh(meshGizmo, spawnPoint.position + gizmoHeightOffset * Vector3.up, Quaternion.identity, Vector3.one * gizmoScale);
    }
}
