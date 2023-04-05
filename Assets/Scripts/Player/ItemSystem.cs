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
        if (context.started && player.heldItem != null && player.holdableItems.Count == 0)
        {
            player.heldItem.GrabRelease(player);
            player.holdableItems.Add(player.heldItem);
            if (player.closestItem == null) player.closestItem = player.heldItem;
            player.heldItem = null;
            return;
        }

        if (context.started && player.closestItem != null && player.closestItem.holdable)
        {
            if(player.heldItem != null)
            {
                //player.holdableItems.Add(player.heldItem);
                player.heldItem.GrabRelease(player);
            }
            if(player.closestItem.GetType() != typeof(Item_Etabli))
            {
                player.heldItem = player.closestItem;
                player.holdableItems.Remove(player.heldItem);
                player.heldItem.GrabStarted(holdPoint, player);
            }
            else
            {
                player.closestItem.GrabStarted(holdPoint, player);
                player.holdableItems.Remove(player.closestItem);
            }
        }
    }

    //throw action
    public void OnItemInput2(InputAction.CallbackContext context)
    {
        if (context.started && player.heldItem != null && player.holdableItems.Count == 0)
        {
            Vector3 direction = transform.forward + Vector3.up * player.throwYAxisDirection;
            player.heldItem.ThrowAction(player, player.throwStrength, direction);
            player.holdableItems.Add(player.heldItem);
            if (player.closestItem == null) player.closestItem = player.heldItem;
            player.heldItem = null;
            return;
        }
    }
}   
