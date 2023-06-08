using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Player_StopDegradation : MonoBehaviour
{
    private Player player;
    [HideInInspector] public RessourcesManager ressourcesManager;
    [SerializeField] private int scoreOnTileElevate;
    private void Start()
    {
        player = GetComponent<Player>();
        ressourcesManager = FindObjectOfType<RessourcesManager>();
    }

    public void OnStopDegrading(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (TileSystem.Instance.isHub && player.tileUnder && player.tileUnder.tileType == TileType.LevelLoader) LoadScene();
            else if(!TileSystem.Instance.isHub) Reperation();
        }
    }

    private void LoadScene()
    {
        if (!TileSystem.Instance.isHub) player.tileUnder.tc.levelUI.DisableUI();
        else TileSystem.Instance.centerTile.tc.levelUI.EnableUI();
        TileSystem.Instance.fileName = player.tileUnder.levelName;
        TileSystem.Instance.centerTile = player.tileUnder;
        StartCoroutine(GridUtils.SinkWorld(player.tileUnder, false, false)) ;
    }

    private void Reperation()
    {
        Tile tile = player.tileUnder;
        if(tile == null)
        {
            return;
        }
        //bool cd1 = player.tileUnder.CompareTag("DegradingTile");
        bool cd3 = !tile.isGrowing;
        bool cd4 = tile.walkable;
        bool cd5 = tile.currentPos.y != GameConstant.tileHeight;
        bool cd6 = tile.degradable;
        bool cd7 = player.heldItem && player.heldItem.GetType() == typeof(Item_Stack);
        if (!cd7) 
        {
            return;
        }
        Item_Stack stackItem = player.heldItem as Item_Stack;
        bool cd8 = stackItem.numberStacked >= ressourcesManager.growthCost;
        if (cd3 && cd4 && cd5 && cd6 && cd7 && cd8)
        {
            stackItem.numberStacked -= ressourcesManager.growthCost;
            tile.ChangeCurrentPos(1);
            //tile.currentPos.y += tile.td.heightByTile;
            tile.isGrowing = true;
            tile.StopDegradation();
            if (!TileSystem.Instance.isHub) TileSystem.Instance.scoreManager.ChangeScore(tile.GetScoreValue());
            StartCoroutine(player.Casting(Player.PlayerState.SpellUp));
        }
    }
}
