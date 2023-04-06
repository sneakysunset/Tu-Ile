using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLineEvents
{
    static public void ApocalypseEvent()
    {
        TileSystem tileS = TileSystem.Instance;
        List<Tile> targettedTiles = tileS.GetTilesAround(20, tileS.centerTile);
        foreach(Tile t in targettedTiles)
        {
            if (t.degradable)
            {
                t.currentPos.y -= t.heightByTile;
            }
        }
    }
}
