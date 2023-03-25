using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetClosestItem : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        foreach (Item item in player.holdableItems) if (item == null) { player.holdableItems.Remove(item); break; };

        if (player.holdableItems.Count > 0) player.closestItem = ClosestItem();
        else player.closestItem = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Item item))
        {
            if (player.heldItem != item && !player.holdableItems.Contains(item) && item.holdable)
            {
                if(item.GetType() == typeof(Item_Stack))
                {
                    Item_Stack item_Stack = (Item_Stack)item;
                    if (!item_Stack.trueHoldable) return;
                }
                GetItemOnTriggerEnter(item);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Item item) && player.holdableItems.Contains(item))
        {
            RemoveItemTriggerExit(item);
                item.Highlight.SetActive(false);
            if (player.holdableItems.Count == 0)
            {
                player.closestItem = null;
            }
        }
    }

    void GetItemOnTriggerEnter(Item item)
    {
        if (player.heldItem != null && item.GetType() == typeof(Item_Stack_Tile))
        {
            Item_Stack_Tile tempItem = item as Item_Stack_Tile;
            Item_Stack_Tile heldItemS = player.heldItem as Item_Stack_Tile;
            if (tempItem.stackType == heldItemS.stackType)
            {
                heldItemS.numberStacked += tempItem.numberStacked;
                Destroy(tempItem.gameObject);
                return;
            }
        }

        if (player.heldItem != null && item.GetType() == typeof(Item_Stack) && player.heldItem.GetType() == typeof(Item_Stack))
        {
            Item_Stack tempItem = item as Item_Stack;
            Item_Stack heldItemS = player.heldItem as Item_Stack;
            if (tempItem.stackType == heldItemS.stackType)
            {
                heldItemS.numberStacked += tempItem.numberStacked;
                Destroy(tempItem.gameObject);
                return;
            }
        }
        player.holdableItems.Add(item);
    }

    void RemoveItemTriggerExit(Item item) => player.holdableItems.Remove(item);


    Item ClosestItem()
    {
        Item cItem = null;
        float distance = Mathf.Infinity;

        if(player.holdableItems.Count > 0)
        {
            foreach (Item item in player.holdableItems)
            {
                 float itemDistance = Vector2.Distance(item.transform.position, transform.position);
                 if (itemDistance < distance)
                 {
                     cItem = item;
                 }
            }
        }

        if (player.closestItem != null && cItem != player.closestItem)
        {
            player.closestItem.Highlight.SetActive(false);
            cItem.Highlight.SetActive(true);
        }
        else if (player.closestItem == null) cItem.Highlight.SetActive(true);

        return cItem;
    }

}
