using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Player_StopDegradation : MonoBehaviour
{
    private Player player;
    bool isGrowing;
    [HideInInspector] public RessourcesManager ressourcesManager;

    private void Start()
    {
        player = GetComponent<Player>();
        ressourcesManager = FindObjectOfType<RessourcesManager>();
        SceneManager.sceneLoaded += OnLoad;
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
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
        if(!TileSystem.Instance.isHub)
        {
            Reperation();
        }
        else
        {
            LoadScene(); 
        }
    }

    private void LoadScene()
    {
        if(player.tileUnder && player.tileUnder.tileType == TileType.LevelLoader && isGrowing)
        {
            StartCoroutine(TileSystem.Instance.SinkWorld(player.tileUnder.levelName));
        }
        isGrowing = false;
    }

    private void Reperation()
    {
        Tile tile = player.tileUnder;
        if(tile == null)
        {
            isGrowing = false;
            return;
        }
        //bool cd1 = player.tileUnder.CompareTag("DegradingTile");
        bool cd2 = isGrowing;
        bool cd3 = !tile.isGrowing;
        bool cd4 = tile.walkable;
        bool cd5 = tile.currentPos.y != GameConstant.tileHeight;
        bool cd6 = tile.degradable;
        bool cd7 = player.heldItem && player.heldItem.GetType() == typeof(Item_Stack);
        if (!cd7) 
        {
            isGrowing = false;
            return;
        }
        Item_Stack stackItem = player.heldItem as Item_Stack;
        bool cd8 = stackItem.numberStacked >= ressourcesManager.growthCost;
        if (cd2 && cd3 && cd4 && cd5 && cd6 && cd7 && cd8)
        {
            stackItem.numberStacked -= ressourcesManager.growthCost;
            tile.currentPos.y += tile.heightByTile;
            tile.isGrowing = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Cast");
        }
        isGrowing = false;
    }
}
