using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

[System.Serializable]
public struct ressourceMeshsCollec
{
    public string name;
    public Item_Stack.StackType stackType;
    public List<Mesh> meshs;
    public List<Material> materials;
    public List<int> necessaryNum;
    public Sprite sprite;
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
    public GameObject[] spawnableItems;
    public List<GameTimer> gameManagers;
    public SO_Recette[] recettes;
    public static RessourcesManager Instance { get; private set; }

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

    public GameObject getSpawnableFromList(string spawnableName)
    {
        foreach(var item in spawnableItems)
        {
            if(item.name == spawnableName) return item;
        }
        return null;
    }

    public SO_Recette getRecetteFromList(string recetteName)
    {
        foreach(var recette in recettes)
        {
            if (recetteName == recette.ToString()) return recette;
        }
        return null;
    }
}
