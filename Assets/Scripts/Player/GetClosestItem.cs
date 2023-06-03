using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetClosestItem : MonoBehaviour
{
    private Player player;

    #region SystemCallBacks
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

    private void OnTriggerStay(Collider other)
    {

        if (other.TryGetComponent(out Item item) && !player.holdableItems.Contains(item))
        {
            if (player.heldItem != item && item.holdable && item.interactable)
            {
                if(item.GetType() == typeof(Item_Stack))
                {
                    Item_Stack item_Stack = (Item_Stack)item;
                    if (!item_Stack.trueHoldable) return;
                }

                else if (Utils.IsSameOrSubclass(item.GetType(), typeof(Item_Etabli)))
                {
                    Item_Etabli item_ = (Item_Etabli)item;
                    if (!player.heldItem || !Utils.CheckIfItemInRecette(player.heldItem, item_.recette)) return;
                }
                GetItemOnTriggerEnter(item);
            }
            
        }
        else if (other.TryGetComponent(out Item_Stack citem) && player.holdableItems.Contains(citem))
        {
            if(player.heldItem && player.heldItem.GetType() == typeof(Item_Stack) && citem.GetType() == typeof(Item_Stack))
            {
                Item_Stack itemSt = player.heldItem as Item_Stack;
                if(itemSt.stackType == citem.stackType)
                {
                    itemSt.numberStacked += citem.numberStacked;
                    Destroy(citem.gameObject);
                    return;
                }
            }
            else if (player.heldItem && player.heldItem.GetType() == typeof(Item_Stack_Tile) && citem.GetType() == typeof(Item_Stack_Tile))
            {
                Item_Stack_Tile itemSt = player.heldItem as Item_Stack_Tile;
                if (itemSt.stackType == citem.stackType)
                {
                    itemSt.numberStacked += citem.numberStacked;
                    Destroy(citem.gameObject);
                    return;
                }
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
    #endregion

    #region Methods
    void GetItemOnTriggerEnter(Item item)
    {
        if (player.heldItem != null && item.GetType() == typeof(Item_Stack_Tile) && player.heldItem.GetType() == typeof(Item_Stack_Tile))
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

        if(player.holdableItems.Count > 0 && TileSystem.Instance.ready)
        {
            foreach (Item item in player.holdableItems)
            {
                if(!item.interactable)
                {
                    player.holdableItems.Remove(item);
                    return null;
                }
                if (item == null) return null;
                 float itemDistance = Vector2.Distance(item.transform.position, transform.position);
                 if (itemDistance < distance )
                 {
                     cItem = item;
                 }
            }
        }


        if (TileSystem.Instance.ready && player.closestItem != null && cItem != player.closestItem)
        {
            player.closestItem.Highlight.SetActive(false);
            cItem.Highlight.SetActive(true);
        }
        else if (TileSystem.Instance.ready && player.closestItem == null) cItem.Highlight.SetActive(true);

        if (cItem && !cItem.Highlight.activeInHierarchy) cItem.Highlight.SetActive(true);

        return cItem;
    }
    #endregion
}
