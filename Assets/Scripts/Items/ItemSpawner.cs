using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    Item spawnedItem;
    public Item itemToSpawn;
    private Transform spawnPoint;
    public float spawnTimer = 3f;
    private float baseTimerValue;
    Tile tile;
    private void Start()
    {
        spawnPoint = transform.Find("SpawnPositions");
        SpawnItem();
        baseTimerValue = spawnTimer;
        tile = GetComponent<Tile>();
    }

    private void SpawnItem()
    {
        spawnedItem = Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity).GetComponent<Item>();
    }

    private void Update()
    {
        if(spawnedItem == null && tile.walkable)
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
