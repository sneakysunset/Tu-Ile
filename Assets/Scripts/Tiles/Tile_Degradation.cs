using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile_Degradation : MonoBehaviour
{
    Tile tile;
    private float degradationTimerModifier;
    [HideNormalInspector] private bool walkedOntoChecker;

    private void Start()
    {
        tile = GetComponent<Tile>();

    }
    private void Update()
    {
        if(tile.isGrowing) Elevating();
        else if (tile.walkable && tile.degradable && tile.walkedOnto && tile.tileType != Tile.TileType.Sand) Degrading();
        if (!tile.isDegrading && tile.walkable && ((transform.position.y <= -tile.heightByTile && tile.currentPos.y <= -tile.heightByTile)))
        {
            SinkTile();
        }
        
        if (tile.tileType == Tile.TileType.Sand)
        {
            SandDegradation();
        }
    }

    private void SandDegradation()
    {
        if(!tile.sand_WalkedOnto && walkedOntoChecker)
        {
            tile.currentPos.y -= tile.heightByTile;
        }

        walkedOntoChecker = tile.sand_WalkedOnto;
    }

    private void Degrading()
    {
        //Effect while degrading
        if (tile.timer > 0)
        {
            tile.timer -= Time.deltaTime * TileSystem.Instance.degradationTimerModifier * tile.typeDegradingSpeed;
        }
        //DegradationStart
        else if (tile.timer <= 0 && !tile.isDegrading)
        {
            tile.isDegrading = true;
            gameObject.tag = "DegradingTile";
            tile.currentPos.y -= tile.heightByTile;
        }

        //DegradationEnd
        if (transform.position == tile.currentPos && tile.isDegrading)
        {
            tile.isDegrading = false;
            tile.timer = Random.Range(tile.minTimer, tile.maxTimer);
        }

        if (tile.currentPos.y == GameConstant.maxTileHeight && CompareTag("DegradingTile"))
        {
            tag = "Tile";
        }
    }
    
    private void Elevating()
    {
        if(transform.position.y == tile.currentPos.y)
        {
            tile.isGrowing = false;
            tile.timer = Random.Range(tile.minTimer, tile.maxTimer);
        }
    }

    void SinkTile()
    {
        tile.walkable = false;
        gameObject.layer = LayerMask.NameToLayer("DisabledTile");
        tile.myMeshR.enabled = false;
        //GetComponent<Collider>().enabled = false;
        transform.Find("Additional Visuals").gameObject.SetActive(false);
        tile.minableItems.gameObject.SetActive(false);
        tile.tileS.tileC.Count();
    }
}
