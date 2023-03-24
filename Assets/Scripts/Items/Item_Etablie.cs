using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct stack
{
    public Item_Stack.StackType stackType;
    public int cost;
    [HideInInspector] public Item_Stack item;
}

public class Item_Etablie : Item
{
    public string recipeName;
    public stack[] requiredItemStacks;
    public Item_Stack craftedItemPrefab;
    Item_Stack craftedItem;
    private Transform stackT;
    private bool convertorFlag;
    private WaitForSeconds waiter;
    public float timeToCraft;
    Transform createdItem;
    public override void Awake()
    {
        base.Awake();

        waiter = new WaitForSeconds(timeToCraft);
        rb.isKinematic = true;
        stackT = transform.Find("Stacks");
        for (int i = 0; i < requiredItemStacks.Length; i++)
        {
            Item_Stack iS = stackT.GetChild(i).AddComponent<Item_Stack>();
            requiredItemStacks[i].item = iS;
            iS.stackType = requiredItemStacks[i].stackType;
            iS.holdable = true;
            iS.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void Start()
    {
        Tile tile = FindObjectOfType<TileSystem>().WorldPosToTile(transform.position);
        transform.parent = tile.transform;
        createdItem = transform.Find("TileCreator/CreatedPos");
    }

    public override void Update()
    {
        base.Update();
        int f = 0;
        foreach(var i in requiredItemStacks)
        {
            if(i.cost <= i.item.numberStacked)
            {
                f++;
            }
        }
        if(f == requiredItemStacks.Length && !convertorFlag)
        {
            StartCoroutine(Convert());
        }
    }

    IEnumerator Convert()
    {
        convertorFlag = true;
        yield return waiter;
        foreach (var i in requiredItemStacks)
        {
            i.item.numberStacked -= i.cost;
        }
        if (createdItem.childCount == 0) CreateStack();

        craftedItem.numberStacked += 1;
        convertorFlag = false;
    }

    void CreateStack()
    {
        craftedItem = Instantiate(craftedItemPrefab, createdItem.position, transform.rotation, null);
        craftedItem.transform.parent = createdItem;
        craftedItem.physic = false;
        craftedItem.rb.isKinematic = true;  
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        if(player.heldItem && player.heldItem.GetType() == typeof(Item_Stack))
        {
            Item_Stack itemS = player.heldItem as Item_Stack;
            foreach(stack iS in requiredItemStacks)
            {
                if(iS.stackType == itemS.stackType)
                {
                    

                    iS.item.numberStacked += itemS.numberStacked;
                    player.heldItem = null;
                    Destroy(itemS.gameObject);
                    Highlight.SetActive(false);
                    break;
                }
            }
        }
    }


}
