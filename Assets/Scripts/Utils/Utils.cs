using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Item;
using Unity.VisualScripting.FullSerializer;

public class Utils
{
    public static bool GetTypeItem(Item.ItemType itemType, Type type1, out Type type)
    {
        bool checker = false;
        switch (itemType)
        {
            case ItemType.Bird: type = Type.GetType("Item_Bird"); break;
            case ItemType.Chantier: type = Type.GetType("Item_Chantier"); break;
            case ItemType.Boussole: type = Type.GetType("Item_Boussole"); break;
            default: type = null; break;
        }

        if (type == type1) checker = true;

        return checker;
    }

    public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
    {
        return potentialDescendant.IsSubclassOf(potentialBase)
               || potentialDescendant == potentialBase;
    }

    public static bool CheckIfItemInRecette(Item item, SO_Recette recette)
    {
        if (IsSameOrSubclass(item.GetType(), typeof(Item_Stack)))
        {
            Item_Stack itS = (Item_Stack)item;
            foreach (stack it in recette.requiredItemStacks)
            {
                if (it.stackType == itS.stackType)
                {
                    return true;
                }
            }

        }
        else
        {
            foreach (var i in recette.requiredItemUnstackable)
            {
                if (GetTypeItem(i.itemType, item.GetType(), out System.Type type))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool[] GetSpawnPositions(int flagNum)
    {
        bool[] bools = new bool[7];
        if(flagNum == -1)
        {
            for (int i = 0; i < bools.Length; i++) bools[i] = true;
            return bools;
        }
        int moduloNum = 64;
        for (int i = bools.Length - 1; i >= 0; i--)
        {
            bools[i] = IterateThroughFlags(ref flagNum, moduloNum);
            moduloNum /= 2;
        }

        return bools;
    }

    private static bool IterateThroughFlags(ref int iteratedNum, int moduloNum)
    {
        if (iteratedNum >= moduloNum)
        {
            iteratedNum -= moduloNum;
            return true;
        }
        else return false;
    }
}

public static class GameConstant
{
    public const float tileHeight = 23f;
    public const int maxTileHeight = 8;
}