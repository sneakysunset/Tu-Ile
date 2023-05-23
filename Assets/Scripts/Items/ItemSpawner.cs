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
        Gizmos.DrawMesh(meshGizmo, spawnPoint.position, Quaternion.identity, Vector3.one);
    }
}
