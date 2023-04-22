using AmplifyShaderEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.RuleTile.TilingRuleOutput;

[System.Serializable]
public struct stack
{
    public Item_Stack.StackType stackType;
    [HideInInspector] public int currentNumber;
    public int cost;
}

[System.Serializable]
public struct item
{
    public Item.ItemType itemType;
    [HideInInspector] public bool isFilled;
}

public class Item_Etabli : Item
{
    public SO_Recette recette;
    Item_Stack craftedItem;
    private UnityEngine.Transform stackT;
    private bool convertorFlag;
    private WaitForSeconds waiter;
    UnityEngine.Transform createdItem;
    private MeshRenderer[] meshs;
    bool isActive;
    Tile tileUnder;
    EtabliCanvas itemNum;
    public override void Awake()
    {
        base.Awake();
        meshs = GetComponentsInChildren<MeshRenderer>();
        waiter = new WaitForSeconds(recette.convertionTime);
        rb.isKinematic = true;
        itemNum = GetComponentInChildren<EtabliCanvas>();
        stackT = transform.Find("Stacks");
    }

    public void SetActiveMesh(bool active)
    {
        foreach(MeshRenderer m in meshs)
        {
            if (active)
            {
                m.enabled = true;
                isActive = true;
                itemNum.gameObject.SetActive(true);
            }
            else
            {
                m.enabled = false;
                isActive = false;
                itemNum.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        tileUnder = FindObjectOfType<TileSystem>().WorldPosToTile(transform.position);
        transform.parent = tileUnder.transform;
        createdItem = transform.Find("TileCreator/CreatedPos");
        itemNum.UpdateText();
    }

    public override void Update()
    {
        base.Update();

        if(isActive && !tileUnder.walkable)
        {
            SetActiveMesh(false);
        }
        else if(!isActive && tileUnder.walkable)
        {
            SetActiveMesh(true);
        }




    }

    IEnumerator Convert()
    {
        convertorFlag = true;
        yield return waiter;
        for (int i = 0; i < recette.requiredItemStacks.Length; i++)
        {
            recette.requiredItemStacks[i].currentNumber -= recette.requiredItemStacks[i].cost;
        }
        int f = 0;
        for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
        {
            recette.requiredItemUnstackable[i].isFilled = false;
            f++;
        }
        craftedItem = Instantiate(recette.craftedItemPrefab, createdItem.position, Quaternion.identity, createdItem);
        craftedItem.numberStacked += recette.numberOfCrafted;
        itemNum.UpdateText();
        convertorFlag = false;  
    }


    public override void GrabStarted(UnityEngine.Transform holdPoint, Player player)
    {
        if(player.heldItem && player.heldItem.GetType() == typeof(Item_Stack))
        {
            Item_Stack itemS = player.heldItem as Item_Stack;
            for (int i = 0; i < recette.requiredItemStacks.Length; i++)
            {
                stack iS = recette.requiredItemStacks[i];
                if(iS.stackType == itemS.stackType)
                {
                    recette.requiredItemStacks[i].currentNumber += itemS.numberStacked;
                    player.heldItem = null;
                    Destroy(itemS.gameObject);
                    Highlight.SetActive(false);
                    break;
                }
            }
        }
        else if (player.heldItem)
        {
            Item itemS = player.heldItem;
            for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
            {
                item iT = recette.requiredItemUnstackable[i];
                if(GetTypeItem(iT.itemType, player.heldItem.GetType(), out System.Type type))
                {
                    recette.requiredItemUnstackable[i].isFilled = true;
                    Item tempItem = player.heldItem;
                    player.heldItem = null;
                    Destroy(tempItem.gameObject);
                    Highlight.SetActive(false);
                    break;
                }
            }
        }
        itemNum.UpdateText();
        if (CheckStacks())
        {
            StartCoroutine(Convert());
        }
    }

    bool CheckStacks()
    {
        int f = 0;
        foreach (var i in recette.requiredItemStacks)
        {
            if (i.cost <= i.currentNumber)
            {
                f++;
            }
        }
        foreach(var i in recette.requiredItemUnstackable)
        {
            if(i.isFilled)
            {
                f++;
            }
        }

        if (f == recette.requiredItemStacks.Length + recette.requiredItemUnstackable.Length && !convertorFlag)
        {
            return true;
        }
        else return false;
    }

    public static bool GetTypeItem(Item.ItemType itemType, System.Type type1, out System.Type type)
    {
        bool checker = false;
        switch (itemType)
        {
            case ItemType.Bird: type = System.Type.GetType("Item_Bird"); break;
            default: type = null; break;
        }

        if (type == type1) checker = true;

        return checker;
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(Item_Etabli))]
[System.Serializable]
public class EtablieSystemEditor : Editor
{
    Item_Etabli etabli;

    private void OnEnable()
    {
        etabli = (Item_Etabli)target;
    }


    private void OnSceneGUI()
    {
        if (!Application.isPlaying)
        {
            Draw();
            EditorUtility.SetDirty(etabli);
        }
    }

    void Draw()
    {
        if (!Application.isPlaying)
        {
            Tile tileUnder = TileSystem.Instance.WorldPosToTile(etabli.transform.position);
            etabli.transform.position = new Vector3(etabli.transform.position.x, tileUnder.transform.position.y + 23.4f, etabli.transform.position.z);
        }
    }
}
#endif
