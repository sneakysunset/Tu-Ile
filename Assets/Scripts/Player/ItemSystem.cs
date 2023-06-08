using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ItemSystem : MonoBehaviour
{
    private Player player;
    public Transform holdPoint;
    private Player_Pause pPause;
    private void Start()
    {
        player = GetComponent<Player>();
        pPause = GetComponent<Player_Pause>();
    }



    //Normal Grab Action
    public void OnItemInput1(InputAction.CallbackContext context)
    {
        bool isSameOrSubClass = player.closestItem != null && Utils.IsSameOrSubclass(player.closestItem.GetType(), typeof(Item_Etabli));
        if (context.started && TileSystem.Instance.isHub && player.tileUnder.tileType == TileType.LevelLoader)
        {
            if (player.tileUnder.isDetail) player.tileUnder.IsDetail = false;
            else player.tileUnder.IsDetail = true;
            return;
        }
        else if (context.started && player.heldItem != null && player.holdableItems.Count == 0 && !isSameOrSubClass && !pPause.isPaused)
        {
            player.heldItem.GrabRelease(false);
            player.holdableItems.Add(player.heldItem);
            if (player.closestItem == null) player.closestItem = player.heldItem;
            player.heldItem = null;

            return;
        }

        else if (context.started && player.closestItem != null && player.closestItem.holdable && !pPause.isPaused)
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


            while(player.interactors.Count > 0)
            {
                player.interactors[0].OnInteractionExit(player);
            }
        }

    }

    //throw action
    public void OnItemInput2(InputAction.CallbackContext context)
    {
        if (context.started && player.heldItem != null && !pPause.isPaused)
        {
            Vector3 direction = transform.forward + Vector3.up * player.throwYAxisDirection;
            player.heldItem.ThrowAction(player, player.throwStrength, direction);
            player.holdableItems.Add(player.heldItem);
            if (player.closestItem == null) player.closestItem = player.heldItem;
            player.heldItem = null;
            return;
        }
        else if(context.started && player.heldItem == null && !pPause.isPaused)
        {
            player.pM.Push();
        }
    }
}   
