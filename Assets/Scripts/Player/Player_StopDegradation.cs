using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_StopDegradation : MonoBehaviour
{
    private Player player;
    bool isGrowing;
    public float growingSpeed;
    public int growPrice;
    public RessourcesManager ressourcesManager;

    private void Start()
    {
        player = GetComponent<Player>();
        ressourcesManager = FindObjectOfType<RessourcesManager>();
    }

    public void OnStopDegrading(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isGrowing = true;
        }
    }

    private void Update()
    {
        Reperation();
    }

    private void Reperation()
    {
        Tile tile = player.tileUnder;
        if(tile == null)
        {
            isGrowing = false;
            return;
        }
        bool cd1 = player.tileUnder.CompareTag("DegradingTile");
        bool cd2 = isGrowing;
        bool cd3 = !tile.isGrowing;
        bool cd4 = tile.walkable;
        bool cd5 = tile.currentPos.y != tile.maxPos;
        bool cd6 = tile.degradable;
        bool cd7 = player.heldItem && player.heldItem.GetType() == typeof(Item_Stack);
        if (!cd7) 
        {
            isGrowing = false;
            return;
        }
        Item_Stack stackItem = player.heldItem as Item_Stack;
        bool cd8 = stackItem.numberStacked >= ressourcesManager.growthCost;
        if (cd1 && cd2 && cd3 && cd4 && cd5 && cd6 && cd7 && cd8)
        {
            stackItem.numberStacked -= ressourcesManager.growthCost;
            tile.currentPos.y += tile.heightByTile;
            tile.isGrowing = true;
        }
        isGrowing = false;
    }
}
