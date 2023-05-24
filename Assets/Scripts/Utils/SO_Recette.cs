using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable Objects/Recettes", menuName = "Recettes/Recette", order = 1)]
public class SO_Recette : ScriptableObject
{
    public float convertionTime;
    public stack[] requiredItemStacks;
    public item[] requiredItemUnstackable;
    public Item craftedItemPrefab;
    public int numberOfCrafted;
}
