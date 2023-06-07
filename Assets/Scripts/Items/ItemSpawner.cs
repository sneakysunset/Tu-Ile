using NaughtyAttributes;
using System;
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
    [ShowIf("itemToSpawn", SpawnableItems.Etabli)] public SO_Recette recette;
    [ShowIf("itemToSpawn", SpawnableItems.Chantier)] public SO_Recette otherRecette;
    public SO_CrateReward crateReward;
    
    private void Start()
    {
        tile = GetComponent<Tile>();
        spawnPoint = transform.Find("SpawnPositions2");
        baseTimerValue = spawnTimer;

        foreach( var item in RessourcesManager.Instance.itemsToSpawn)
        {
            if(item.item.name == itemToSpawn.ToString())
            {
                poolIndex = item.index;
                chosenItemToSpawn = item.item;
                return;
            }

        }
    }

    private void SpawnItem()
    {
/*        SO_Recette tempRecette = null;
        SO_CrateReward tempReward = null;
        if (itemToSpawn == SpawnableItems.Etabli) tempRecette = recette;
        else if(itemToSpawn == SpawnableItems.Chantier) tempRecette = otherRecette;
        else if(itemToSpawn == SpawnableItems.Crate || itemToSpawn == SpawnableItems.Mimic) tempReward = crateReward;*/
        //Vector3 pos = spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 30 * Vector3.up;
        //spawnedItem = ObjectPooling.SharedInstance.GetPoolItem(poolIndex, pos, null, null, tempRecette, crateReward);
        spawnedItem = Instantiate(chosenItemToSpawn);
        spawnedItem.transform.position =  spawnPoint.GetChild((int)spawnPosition).position + chosenItemToSpawn.transform.position + 30 * Vector3.up;
        switch (itemToSpawn)
        {
            case SpawnableItems.Etabli:
                Item_Etabli it = spawnedItem.GetComponent<Item_Etabli>();
                it.recette = recette;
                //print(it.recette);
                StartCoroutine(it.Enable());
                break;
            case SpawnableItems.Chantier:
                Item_Etabli itc = spawnedItem.GetComponent<Item_Etabli>();
                itc.recette = otherRecette;
                //print(itc.recette);
                StartCoroutine(itc.Enable());

                break;
            case SpawnableItems.Crate:
                Item_Crate itcr = spawnedItem.GetComponent<Item_Crate>();
                itcr.reward = crateReward;
                break;
            case SpawnableItems.Mimic:
                Item_Crate_Mimic itcrm = spawnedItem.GetComponent<Item_Crate_Mimic>();
                itcrm.reward = crateReward;
                break;
        }
        if (!loop) { continueLooping = false; }
    }

    private void LateUpdate()
    {
        if((TileSystem.Instance.ready || (itemToSpawn == SpawnableItems.Chantier || itemToSpawn == SpawnableItems.Etabli)) && spawnedItem == null && tile.walkable && continueLooping)
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
