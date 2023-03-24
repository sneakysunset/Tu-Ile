using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile_Degradation : MonoBehaviour
{
    Tile tile;
    bool degradingChecker;
    bool isGrowingChecker;
    private float degradationTimerModifier;

    private void Start()
    {
        tile = GetComponent<Tile>();
    }
    private void Update()
    {
        if(tile.isGrowing) Elevating();
        else if (tile.walkable && tile.degradable && tile.walkedOnto) Degrading();
        if (!tile.isDegrading && transform.position.y <= -tile.heightByTile)
        {
            SinkTile();
        }
    }


    private void Degrading()
    {
        degradationTimerModifier += Time.deltaTime * (1 / tile.timeToGetToMaxDegradationSpeed);


        //Effect while degrading
        if (tile.timer > 0)
        {
            tile.timer -= Time.deltaTime * tile.degradationTimerAnimCurve.Evaluate(degradationTimerModifier);
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

        if (tile.currentPos.y == tile.maxPos && CompareTag("DegradingTile"))
        {
            tag = "Tile";
        }


        degradingChecker = tile.isDegrading;
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
    }
}
