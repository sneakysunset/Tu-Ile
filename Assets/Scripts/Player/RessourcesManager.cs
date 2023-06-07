using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Complete;

[System.Serializable]
public struct ressourceMeshsCollec
{
    public string name;
    public Item_Stack.StackType stackType;
    public List<Mesh> meshs;
    public List<Material> materials;
    public List<int> necessaryNum;
    public Sprite sprite;
    public Sprite tileSprite;

}

[System.Serializable]
public struct ressourceMeshCollecUnstackable
{
    public string name;
    public Item_Stack.ItemType itemType;
    public Sprite sprite;
    public Mesh mesh;
    public Material mat;
}

[System.Serializable]
public struct recetteResultCollec
{
    public string name;
    public bool isTile;
    public Item_Etabli.StackType tileType;
    public Item_Etabli.ItemType itemType;

    public Sprite sprite;
}

[System.Serializable]
public struct ItemToSpawn
{
    public GameObject item;
    public int index;
}

[System.Serializable]
public struct tileMeshs
{
    public Mesh mesh;
    public Item.StackType stackType;
}

public class RessourcesManager : MonoBehaviour
{
    public int growthCost;
    public ressourceMeshsCollec[] RessourceMeshs;
    public ressourceMeshCollecUnstackable[] RessourceMeshsUnstackable;
    public recetteResultCollec[] ressourceRecettesResults;
    //public Sprite[] mSTileCreation;
/*    public Sprite mChickenElim;
    public Sprite mCompass;
    public Sprite mConstr;*/
    public ItemToSpawn[] itemsToSpawn;
    public List<GameTimer> gameManagers;
    public SO_Recette[] recettes;
    public SO_CrateReward[] rewards;
    public tileMeshs[] meshs;
    public static RessourcesManager Instance { get; private set; }
    

    public Mesh GetMeshByStackType(Item.StackType _stackType)
    {
        foreach(tileMeshs tmesh in meshs)
        {
            if (_stackType == tmesh.stackType) return tmesh.mesh;
        }
        return null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public GameTimer getGameManagerFromList(string gameManName)
    {
        foreach (var item in gameManagers)
        {
            if (item.gameObject.name == gameManName) return item;
        }
        return null;
    }

    public GameObject getSpawnableFromList(string spawnableName)
    {
        foreach(var item in itemsToSpawn)
        {
            if(item.item.name == spawnableName) return item.item;
        }
        return null;
    }

    public SO_Recette getRecetteFromList(string recetteName)
    {
        foreach(var recette in recettes)
        {
            if (recetteName.Split(' ')[0] == recette.name)
            {
                return recette;
            }
        }
        return null;
    }

    public SO_CrateReward getRewardFromList(string rewardName)
    {
        foreach (var reward in rewards)
        {
            if (rewardName.Split(' ')[0] == reward.name)
            {
                return reward;
            }
        }
        return null;
    }

    public void SpawnRessources(int type)
    {
        if(!TileSystem.Instance.isHub) 
        {         
            GameObject obj = Instantiate(itemsToSpawn[type].item, FindObjectOfType<Player>().transform.position + Vector3.up * 30,  Quaternion.identity);
            if (obj.TryGetComponent(out Item_Stack itemS)) itemS.numberStacked = 50;
        }
    }
}
