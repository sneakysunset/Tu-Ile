using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TileSelector : MonoBehaviour
{
    TileSystem tileS;
    public GameObject bluePrintPrefab;
    Transform tileBluePrint;
    public float maxAngleToTarget = 50;
    public LayerMask tileLayer;
    public float hitDistance = 4;
    private Tile targettedTile;
    private Tile previousTile;
    private Player player;
    private MissionManager mM;
    private void Start()
    {
        tileBluePrint = Instantiate(bluePrintPrefab).transform;
        tileS = FindObjectOfType<TileSystem>();
        player = GetComponent<Player>();
        mM = FindObjectOfType<MissionManager>();
        SceneManager.sceneLoaded += OnLoad;
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        tileBluePrint = Instantiate(bluePrintPrefab).transform;
        tileS = FindObjectOfType<TileSystem>();
        mM = FindObjectOfType<MissionManager>();
    }

    private void OnChangeTileUnder()
    {
        if(player.tileUnder.etabli != null)
        {
            player.tileUnder.etabli.playersOn.Add(player);
            if (player.tileUnder.etabli.playersOn.Count == 1) player.tileUnder.etabli.PlayerNear();
        }
        if(previousTile && previousTile.etabli != null)
        {
            previousTile.etabli.playersOn.Remove(player);
            previousTile.sand_WalkedOnto = false;
            if (previousTile.etabli.playersOn.Count == 0) previousTile.etabli.PlayerFar();
        }
    }

    private void Update()
    {
        if(player.tileUnder) player.tileUnder.sand_WalkedOnto = false;
        player.tileUnder = TileSystem.Instance.WorldPosToTile(transform.position);
        if(player._characterController.isGrounded && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(player.tileUnder.transform.position.x, player.tileUnder.transform.position.z)) < 2)
        {
            player.tileUnder.sandFlag = true;
        }
        if(player._characterController.isGrounded) player.tileUnder.sand_WalkedOnto = true;
            

        if (previousTile != player.tileUnder)
        {
            OnChangeTileUnder();
        }

        previousTile = player.tileUnder;
        if (!player.tileUnder.walkedOnto)
        {
            player.tileUnder.walkedOnto = true;
        }
        if (Physics.Raycast(player.tileUnder.transform.position, transform.forward, out RaycastHit hit, hitDistance, tileLayer) && hit.transform.TryGetComponent<Tile>(out targettedTile) && !targettedTile.walkable && !targettedTile.tourbillon && player.heldItem && player.heldItem.GetType() == typeof(Item_Stack_Tile))
        {
            tileBluePrint.position = new Vector3(targettedTile.transform.position.x, (GameConstant.tileHeight + player.tileUnder.transform.position.y), targettedTile.transform.position.z) ;
        }
        else
        {
            tileBluePrint.position = new Vector3(0, -100, 0);
            targettedTile = null; 
        }
    }


    public void OnSpawnTile(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if (player.heldItem && player.heldItem.GetType() == typeof(Item_Stack_Tile) && targettedTile != null && !targettedTile.tourbillon)
            {
                Item_Stack_Tile item = player.heldItem as Item_Stack_Tile;
                if(item.numberStacked >= 1)
                {
                    item.numberStacked --;
                    targettedTile.Spawn(player.tileUnder.transform.position.y, item.stackType.ToString(), item.degradingSpeed) ;
                    if(item.numberStacked == 0)
                    {
                        player.heldItem = null;
                        Destroy(item.gameObject);
                    }
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Cast");
                }
            }
            else if(player.heldItem && player.heldItem.GetType() == typeof(Item_Boussole))
            {
                Item_Boussole _item = player.heldItem as Item_Boussole;
                foreach(Tile tile in _item.targettedTiles)
                {
                    if(tile == player.tileUnder)
                    {
                        for (int i = 0; i < mM.activeMissions.Length; i++)
                        {
                            if (mM.activeMissions[i].boussoleTile && tile == mM.activeMissions[i].boussoleTile)
                            {
                                mM.activeMissions[i].tresorFound = true;
                            }
                        }
                        mM.CheckMissions();
                    }
                }
            }
        }
    }
}
