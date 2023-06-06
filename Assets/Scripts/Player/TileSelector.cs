using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class TileSelector : MonoBehaviour
{
    #region Variables
    public GameObject bluePrintPrefab;
    [HideInInspector] public Transform tileBluePrint;
    public float maxAngleToTarget = 50;
    public LayerMask tileLayer;
    public float hitDistance = 4;
    private Tile targettedTile;
    private Tile previousTile;
    private bool isOnTop;
    private bool IsOnTop { get { return isOnTop; } set { if (isOnTop != value && !value) ExitTop(value); else if (isOnTop != value) EnterTop(value); } }
    public float distanceToBeOnTop = 2.5f;
    private Player player;
    private MissionManager mM;
    public delegate void MissionComplete(Tile tile);
    public static event MissionComplete missionComplete;
    [SerializeField] private int scoreOnTileCreation;
    #endregion

    #region System CallBacks
    private void Start()
    {
        tileBluePrint = Instantiate(bluePrintPrefab).transform;
        player = GetComponent<Player>();
        mM = FindObjectOfType<MissionManager>();
    }


    private void Update()
    {
        GetTileUnder();
        BluePrintPlacement();
    }
    #endregion


    #region TileUnderCallBacks
    private void GetTileUnder()
    {
        player.tileUnder = GridUtils.WorldPosToTile(transform.position);
        IsOnTop = player.tileUnder != null && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(player.tileUnder.transform.position.x, player.tileUnder.transform.position.z)) < distanceToBeOnTop;

        if (player.tileUnder != null) OnTileStay(player.tileUnder);
        if (player.tileUnder != null && previousTile != null && previousTile != player.tileUnder)
        {
            OnTileExit(previousTile);
            OnTileEnter(player.tileUnder);
        }
        previousTile = player.tileUnder;
    }

    private void OnTileEnter(Tile tile)
    {
        #region HUB
        //Activate NearEtabliUI
        #endregion

        #region nonHub

        #endregion

        player.groundType = getTileType(tile.tileType);
    }

    private void OnTileExit(Tile tile)
    {
        #region HUB
        //Disable levelUI
        if (tile.tc.levelUI != null)
        {
            tile.IsNear = false;
            tile.IsDetail = false;
        }

        #endregion

        #region nonHub
        //Disable Etablie Near
        if (tile.etabli != null)
        {
            tile.etabli.playersOn.Remove(player);
            if (tile.etabli.playersOn.Count == 0) tile.etabli.PlayerFar();
        }
        //Sand Tile Got Down
        if (tile.tileType == TileType.Sand && tile.sandFlag) tile.td.SandDegradation();
        tile.sandFlag = false;
        #endregion
    }

    private void OnTileStay(Tile tile)
    {
        #region HUB
        #endregion

        #region nonHub
        if (tile.tileType == TileType.Sand && !player._characterController.isGrounded && tile.sandFlag) tile.td.SandDegradation();
        else if (isOnTop && player._characterController.isGrounded) tile.sandFlag = true;
        #endregion

    }

    private void ExitTop(bool value)
    {
        isOnTop = value;
    }

    private void EnterTop(bool value)
    {
        if (player.tileUnder.tileType == TileType.LevelLoader) player.tileUnder.IsNear = true;
        isOnTop = value;
        player.tileUnder.walkedOnto = true;
        if (player.tileUnder.etabli != null)
        {
            player.tileUnder.etabli.playersOn.Add(player);
            if (player.tileUnder.etabli.playersOn.Count == 1) player.tileUnder.etabli.PlayerNear();
        }

    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Application.isPlaying && player.tileUnder)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(player.tileUnder.tc.minableItems.position, Vector3.up, distanceToBeOnTop);
        }
#endif

        if (Application.isPlaying && player.tileUnder)
        {
            //Gizmos.DrawRay(transform.position + hitDistance * transform.forward, -Vector3.up * 2000);
            //Gizmos.DrawSphere(transform.position + transform.forward * hitDistance, .1f);
        }
    }
    #endregion

    private string getTileType(TileType tileUnderType)
    {
        string groundTypeName = "Neutral";
        switch (player.tileUnder.tileType)
        {
            case TileType.Wood: groundTypeName = player.tileUnder.tileType.ToString(); break;
            case TileType.Rock: groundTypeName = player.tileUnder.tileType.ToString(); break;
            case TileType.Sand: groundTypeName = player.tileUnder.tileType.ToString(); break;
            case TileType.Gold: groundTypeName = "Rock"; break;
            case TileType.Diamond: groundTypeName = "Rock"; break;
            default: break;
        }
        return groundTypeName;
    }
    #region Tile Spawning
    private void BluePrintPlacement()
    {
        bool c3 = player.heldItem && player.heldItem.GetType() == typeof(Item_Stack_Tile);
        if (!c3) goto NotHit;
        Vector3 originCast = transform.position + transform.forward * hitDistance;
        targettedTile = GridUtils.WorldPosToTile(originCast);
        bool c2 = (!targettedTile.walkable && !targettedTile.tourbillon && !TileSystem.Instance.isHub) || (TileSystem.Instance.isHub && targettedTile.coordX == 17 && targettedTile.coordY == 6);
        if (c2  && c3)
        {
            tileBluePrint.position = new Vector3(targettedTile.transform.position.x, (player.tileUnder.transform.position.y), targettedTile.transform.position.z);
            
            return;
        }
        else goto NotHit;

        NotHit:
        if(tileBluePrint.position.y != -100) tileBluePrint.position = new Vector3(0, -100, 0);
        targettedTile = null;
    }

    public void OnSpawnTile(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if (player.heldItem && player.heldItem.GetType() == typeof(Item_Stack_Tile) && targettedTile != null && !targettedTile.tourbillon)
            {
                Item_Stack_Tile item = player.heldItem as Item_Stack_Tile;
                if (item.numberStacked >= 1)
                {
                    item.numberStacked--;
                    
                    targettedTile.Spawn(player.tileUnder.transform.position.y, item.stackType.ToString());
                    StartCoroutine(player.Casting(Player.PlayerState.SpellCreate));
                    if (item.numberStacked == 0)
                    {
                        player.heldItem = null;
                        Destroy(item.gameObject);
                    }
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Cast");
                }
            }
            else if (player.heldItem && player.heldItem.GetType() == typeof(Item_Boussole))
            {
                Item_Boussole _item = player.heldItem as Item_Boussole;
                CompassMissionManager cmm = TileSystem.Instance.compassManager;
                foreach (Tile tile in _item.targettedTiles)
                {
                    if (tile == player.tileUnder)
                    {
                        for(int i = 0; i < cmm.activeM.Count; i++)
                        {
                            if (cmm.activeM[i].targettedTile == tile)
                            {
                                cmm.CompleteMission(tile);
                            }
                        }
                        for (int i = 0; i < mM.activeMissions.Length; i++)
                        {
                            if (mM.activeMissions[i].boussoleTile && tile == mM.activeMissions[i].boussoleTile)
                            {
                                mM.activeMissions[i].tresorFound = true;
                                if (missionComplete != null) missionComplete(player.tileUnder);
                            }
                        }

                        TileSystem.Instance.compassManager.CompleteMission(tile);
                        break;

                        //mM.CheckMissions();
                    }
                }
            }
        }
    }
    #endregion
}
