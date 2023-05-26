using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SpawnableItems
{
    Bird = 1,
    Mimic = 2,
    Chantier = 3,
    Etabli = 4,
    Crate = 5,
    Bait = 6
}

public class ItemSpawner : MonoBehaviour
{
    GameObject spawnedItem;
    public SpawnableItems itemToSpawn;
    [HideNormalInspector] public GameObject chosenItemToSpawn;
    public SpawnPosition spawnPosition;
    private Transform spawnPoint;
    public float spawnTimer = 3f;
    private float baseTimerValue;
    public bool loop;
    bool continueLooping = true;
    Tile tile;
    [Foldout("Gizmo")] public float gizmoScale = 1;
    [Foldout("Gizmo")] public float gizmoHeightOffset = 0;
    [ShowIf("itemToSpawn",SpawnableItems.Etabli)]public SO_Recette recette;
    [ShowIf("itemToSpawn",SpawnableItems.Chantier)]public SO_Recette_Chantier otherRecette;
    
    private void Start()
    {
        tile = GetComponent<Tile>();
        spawnPoint = transform.Find("SpawnPositions2");
        baseTimerValue = spawnTimer;
    }

    private void SpawnItem()
    {

        spawnedItem = Instantiate(chosenItemToSpawn, spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 10 * Vector3.up, Quaternion.identity);

        if (!loop) { continueLooping = false; }
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
        if (chosenItemToSpawn == null)
        {
            if(spawnPoint == null) spawnPoint = transform.GetChild(0);
            Gizmos.DrawCube(spawnPoint.position + gizmoHeightOffset * Vector3.up, Vector3.one * gizmoScale);
            return;
        }
        if(meshGizmo == null || spawnPoint == null)
        {
            meshGizmo = chosenItemToSpawn.transform.Find("Highlight").GetComponent<MeshFilter>().sharedMesh;
            spawnPoint = transform.Find("SpawnPositions");
        }
        Gizmos.DrawMesh(meshGizmo, spawnPoint.position + gizmoHeightOffset * Vector3.up, Quaternion.identity, Vector3.one * gizmoScale);
    }
}
