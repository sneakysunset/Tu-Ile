using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSystem : MonoBehaviour
{
    private Player player;
    Item myItem;
    public Transform holdPoint;

    private void Start()
    {
        player = GetComponent<Player>();
        myItem = GetComponent<Item>();
    }

    //normal grab action
    public void OnItemInput1(InputAction.CallbackContext context)
    {
        if (context.started && player.heldItems.Count != 0 && (player.holdableItems.Count == 0 || player.heldItems[0].itemType != player.closestItem.itemType))
        {
            foreach(Item item in player.heldItems)
            {
                item.GrabRelease(player);
                player.holdableItems.Add(item);
                if (player.closestItem = null) player.closestItem = item;
            }
            player.heldItems.Clear();
            return;
        }

        if (context.started && player.closestItem != null && player.closestItem.holdable)
        {
            player.heldItems.Add(player.closestItem);
            foreach (Item item in player.heldItems)
            {
                player.holdableItems.Remove(item);
                item.GrabStarted(holdPoint, player);
            }
        }
    }
}   
