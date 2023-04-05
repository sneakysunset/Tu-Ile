using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCounter : MonoBehaviour
{
    [Header("Stats")]
    [Space(5)]
    public int numberOfTilesLinked;
    public int neutralCount;
    public int woodCount;
    public int rockCount;
    public int goldCount;
    public int diamondCount;
    public int adamantiumCount;

    private TileSystem tileSystem;
    private MissionManager missionManager;
    private void Start()
    {
        missionManager = FindObjectOfType<MissionManager>();
        tileSystem = GetComponent<TileSystem>();
    }

    public int GetStat(Tile.TileType tType)
    {
        int returnValue = 0;
        switch (tType)
        {
            case Tile.TileType.Neutral:
                returnValue = neutralCount;
                break;
            case Tile.TileType.Wood:
                returnValue = woodCount;
                break;
            case Tile.TileType.Rock:
                returnValue = rockCount;
                break;
            case Tile.TileType.Gold:
                returnValue = goldCount;
                break;
            case Tile.TileType.Diamond:
                returnValue = diamondCount;
                break;
            case Tile.TileType.Adamantium:
                returnValue = adamantiumCount;
                break;
        }
        return returnValue;
    }


    public void Count()
    {
        List<Tile> tiles = tileSystem.GetTilesAround(20, tileSystem.centerTile);
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
                case Tile.TileType.Neutral: neutNum++;
                break;
                case Tile.TileType.Wood: woodNum++;
                break;
                case Tile.TileType.Rock: rockNum++;
                break; 
                case Tile.TileType.Gold: goldNum++;
                break; 
                case Tile.TileType.Diamond: diaNum++;
                break;
                case Tile.TileType.Adamantium: adamNum++;
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
        missionManager.CheckMissions();
    }
}
