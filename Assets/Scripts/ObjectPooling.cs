using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public string itemType;
    public int index;
    public List<GameObject> objects;
    public int numOfItem;
    public GameObject prefab;
}

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling SharedInstance;
    public List<ObjectPool> pooledObjects;
    public Transform poolFolder;

    void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        int index = 0;
        foreach(ObjectPool pool in pooledObjects)
        {
            pool.index = index;
            index++;
            
            int size = 0;
            string strType = pool.itemType.Split(':')[0];
            
            if (strType == "Interactor")
            {
                foreach (Interactor inte in FindObjectsOfType<Interactor>()) if (inte.type.ToString() == pool.itemType.Split(':')[1]) size++;
            }
            else if(strType == "Item_Etabli")
            {
                foreach (Item_Etabli inte in FindObjectsOfType<Item_Etabli>())
                {
                    if (inte.isChantier && pool.itemType.Split(':')[1] == "Chantier") size++;
                    else if (inte.isChantier && pool.itemType.Split(':')[1] == "Etabli") size++;
                }
            }
            else
            {
                size = FindObjectsOfType(System.Type.GetType(strType)).Length;
            }
            if(size < pool.numOfItem)
            {
                for(int i = size;  i < pool.numOfItem; i++)
                {
                    GameObject item = Instantiate(pool.prefab);
                    item.SetActive(false);
                    item.transform.parent = poolFolder;
                    pool.objects.Add(item);
                }
            }
            else
            {
                pool.numOfItem = size;
            }
        }
    }

    public GameObject GetPoolItem(int index,Vector3 pos, Transform parentP, string type = null, SO_Recette optionalRecette = null, SO_CrateReward crateReward = null)
    {
        if (type != null)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (type == pooledObjects[i].itemType) index = i;
            }
        }
        GameObject go ;


        if (pooledObjects[index].objects.Count == 0) go = Instantiate(pooledObjects[index].prefab);
        else
        {
            go = pooledObjects[index].objects[0];
            pooledObjects[index].objects.RemoveAt(0);

        }

        if(optionalRecette != null) go.GetComponent<Item_Etabli>().recette = optionalRecette;
        else if(crateReward != null) go.GetComponent<Item_Crate>().reward = crateReward;
        go.transform.parent = parentP;
        go.transform.position = pos;
        go.SetActive(true);
        
        return go;
    }

    public void RemovePoolItem(int index, GameObject obj, string type = null)
    {
        if(type != null)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (type == pooledObjects[i].itemType.Split(':')[0]) index = i;
            }
        }
        pooledObjects[index].objects.Add(obj);
        obj.SetActive(false);
        obj.transform.parent = poolFolder;
    }
}
