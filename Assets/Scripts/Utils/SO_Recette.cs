using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable Objects", menuName = "Recettes", order = 1)]
public class SO_Recette : ScriptableObject
{
    public float convertionTime;
    public stack[] requiredItemStacks;
    public item[] requiredItemUnstackable;
    public Item_Stack craftedItemPrefab;
    public int numberOfCrafted;

    public void ResetRecette()
    {
        for (int i = 0; i < requiredItemStacks.Length; i++)
        {
            requiredItemStacks[i].currentNumber = 0;
        }

        for (int i = 0;i < requiredItemUnstackable.Length; i++)
        {
            requiredItemUnstackable[i].isFilled = false;
        }
    }
}
