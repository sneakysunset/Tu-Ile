using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCounter : MonoBehaviour
{
    [HideNormalInspector] public int numberOfTilesLinked;
    [HideNormalInspector] public int neutralCount;
    [HideNormalInspector] public int woodCount;
    [HideNormalInspector] public int rockCount;
    [HideNormalInspector] public int goldCount;
    [HideNormalInspector] public int diamondCount;
    [HideNormalInspector] public int adamantiumCount;

    public int GetStat(TileType tType)
    {
        int returnValue = 0;
        switch (tType)
        {
            case TileType.Neutral:
                returnValue = neutralCount;
                break;
            case TileType.Wood:
                returnValue = woodCount;
                break;
            case TileType.Rock:
                returnValue = rockCount;
                break;
            case TileType.Gold:
                returnValue = goldCount;
                break;
            case TileType.Diamond:
                returnValue = diamondCount;
                break;
            case TileType.Adamantium:
                returnValue = adamantiumCount;
                break;
        }
        return returnValue;
    }


    public void Count()
    {
        //Debug.Log(TileSystem.Instance.centerTile);
        List<Tile> tiles = GridUtils.GetTilesAround(20, TileSystem.Instance.centerTile);
        int num = 0;
        int neutNum = 0;
        int woodNum = 0;
        int rockNum = 0;
        int goldNum = 0;
        int diaNum = 0;
        int adamNum = 0;
        foreach( Tile tile in tiles )
        {
            num++;
            switch(tile.tileType)
            {
                case TileType.Neutral: neutNum++;
                break;
                case TileType.Wood: woodNum++;
                break;
                case TileType.Rock: rockNum++;
                break; 
                case TileType.Gold: goldNum++;
                break; 
                case TileType.Diamond: diaNum++;
                break;
                case TileType.Adamantium: adamNum++;
                break;
            }
        }
        numberOfTilesLinked = num;
        neutralCount = neutNum;
        woodCount = woodNum;
        rockCount = rockNum;
        goldCount = goldNum;
        diamondCount = diaNum;
        adamantiumCount = adamNum;
        //missionManager.CheckMissions();
    }
}
