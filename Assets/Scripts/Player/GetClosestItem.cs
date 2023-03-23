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
        foreach (Item item in player.holdableItems) if (item == null) Debug.LogError("NullItem");

        if (player.holdableItems.Count > 0) player.closestItem = ClosestItem();
        else player.closestItem = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Item item))
        {
            if (player.heldItem != item  && !player.holdableItems.Contains(item) && item.holdable)
                GetItemOnTriggerEnter(item);
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

    void GetItemOnTriggerEnter(Item item) => player.holdableItems.Add(item);

    void RemoveItemTriggerExit(Item item) => player.holdableItems.Remove(item);


    Item ClosestItem()
    {
        Item cItem = null;
        float distance = Mathf.Infinity;

        foreach (Item item in player.holdableItems)
        {
             float itemDistance = Vector2.Distance(item.transform.position, transform.position);
             if (itemDistance < distance)
             {
                 cItem = item;
                 if(player.heldItem != null && item.GetType() == typeof(Item_Stack))
                 {
                    Item_Stack temItem = item as Item_Stack;
                    Item_Stack heldItemS = player.heldItem as Item_Stack;
                    if(temItem.stackType == heldItemS.stackType)
                    {
                        player.holdableItems.Remove(item);
                        heldItemS.numberStacked += temItem.numberStacked;
                        Destroy(temItem.gameObject);
                    }
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
