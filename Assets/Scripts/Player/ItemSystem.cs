using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSystem : MonoBehaviour
{
    private Player player;
    public Transform holdPoint;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    //normal grab action
    public void OnItemInput1(InputAction.CallbackContext context)
    {
        bool isSameOrSubClass = player.closestItem != null && Utils.IsSameOrSubclass(player.closestItem.GetType(), typeof(Item_Etabli));
        if (context.started && player.heldItem != null && player.holdableItems.Count == 0 && !isSameOrSubClass)
        {
            player.heldItem.GrabRelease(false);
            player.holdableItems.Add(player.heldItem);
            if (player.closestItem == null) player.closestItem = player.heldItem;
            player.heldItem = null;
            player.interactors.Clear();
            return;
        }

        if (context.started && player.closestItem != null && player.closestItem.holdable)
        {
            if(player.heldItem != null && !isSameOrSubClass)
            {
                //player.holdableItems.Add(player.heldItem);
                player.heldItem.GrabRelease(false);
            }
            if(player.closestItem.GetType() != typeof(Item_Etabli))
            {
                if (player.closestItem.isHeld)
                {
                    player.closestItem.GrabRelease(false);
                    Player pl = player.closestItem._player;
                    pl.holdableItems.Add(player.closestItem);
                    pl.heldItem = null;
                }
                player.heldItem = player.closestItem;
                player.holdableItems.Remove(player.heldItem);
                player.heldItem.GrabStarted(holdPoint, player);
            }
            else
            {

                player.closestItem.GrabStarted(holdPoint, player);
                player.holdableItems.Remove(player.closestItem);
            }

            for (int i = 0; i < player.interactors.Count; i++)
            {
                player.interactors.RemoveAt(i);
                i--;
                if (player.interactors.Count == 0) break;
            }

            foreach(Interactor inte in player.interactors)
            {
                inte.OnInteractionExit(player);
            }
        }

    }

    //throw action
    public void OnItemInput2(InputAction.CallbackContext context)
    {
        if (context.started && player.heldItem != null /*&& player.holdableItems.Count == 0*/)
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
