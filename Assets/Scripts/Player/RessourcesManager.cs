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
    public List<ressourceMeshsCollec> RessourceMeshs;
    public List<ressourceMeshCollecUnstackable> RessourceMeshsUnstackable;
    public List<recetteResultCollec> ressourceRecettesResults;
    public List<Sprite> mSTileCreation;
    public Sprite mChickenElim;
    public Sprite mCompass;
    public Sprite mConstr;
}
