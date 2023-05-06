using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable Objects", menuName = "Recettes", order = 1)]
public class SO_Recette : ScriptableObject
{
    public float convertionTime;
    public stack[] requiredItemStacks;
    public Item_Stack craftedItemPrefab;
    public int numberOfCrafted;
}
