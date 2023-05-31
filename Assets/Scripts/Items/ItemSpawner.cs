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
    Bait = 6,
    Boussole = 7
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
    int poolIndex;
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
/*        if(chosenItemToSpawn == null)
        {*/
            foreach( var item in RessourcesManager.Instance.itemsToSpawn)
            {
                if(item.item.name == itemToSpawn.ToString())
                {

                    //Debug.Log(itemToSpawn.ToString() + " " + poolIndex);
                    poolIndex = item.index;
                    chosenItemToSpawn = item.item;
                    return;
                }

            }
            //Debug.Log(gameObject.name);
        //}
/*        else if (chosenItemToSpawn != null)
        {
            foreach (var item in RessourcesManager.Instance.itemsToSpawn)
            {
                if (item.item.name == chosenItemToSpawn.GetComponent<Item>().getType.ToString())
                {

                    Debug.Log(itemToSpawn.ToString() + " " + poolIndex);
                    poolIndex = item.index;
                    chosenItemToSpawn = item.item;
                    return;
                }

            }
            //Debug.Log(gameObject.name);
        }*/
    }

    private void SpawnItem()
    {

        //spawnedItem = Instantiate(chosenItemToSpawn, spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 30 * Vector3.up, Quaternion.identity);
        SO_Recette tempRecette = null;
        if (itemToSpawn == SpawnableItems.Etabli) tempRecette = recette;
        else if(itemToSpawn == SpawnableItems.Chantier) tempRecette = otherRecette;
        Vector3 pos = spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 30 * Vector3.up;
        spawnedItem = ObjectPooling.SharedInstance.GetPoolItem(poolIndex, pos, null, null, tempRecette);
        
        spawnedItem.transform.position =  spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 30 * Vector3.up;
        if (itemToSpawn == SpawnableItems.Etabli)
        {
            StartCoroutine(spawnedItem.GetComponent<Item_Etabli>().OnPooled(recette));
        }
        else if (itemToSpawn == SpawnableItems.Chantier)
        {
            StartCoroutine(spawnedItem.GetComponent<Item_Etabli>().OnPooled(otherRecette));    
        }
        if (!loop) { continueLooping = false; }
    }

    private void LateUpdate()
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

    private void OnDrawGizmos()
    {

            if(spawnPoint == null) spawnPoint = transform.GetChild(0);
            Gizmos.DrawCube(spawnPoint.position + gizmoHeightOffset * Vector3.up, Vector3.one * gizmoScale);
            return;
    }
}
