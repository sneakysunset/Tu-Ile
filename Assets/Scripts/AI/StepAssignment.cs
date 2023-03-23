using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class StepAssignment 
{
    static public List<Tile> Initialisation(Tile targetTile, TileSystem tileSystem, Vector3 ogPos)
    {
        List<Tile> result = new List<Tile>();
        Tile[,] tiles = tileSystem.tiles;
        foreach (Tile obj in tiles)
        {
            obj.step = -1;
            if (!obj.walkable)
            {
                obj.step = -2;
            }
        }
        targetTile.step = 0;
        AssignationChecker(tileSystem, tiles);
        Tile startTile = tileSystem.WorldPosToTile(ogPos);
        int startStep = startTile.step;
        CreatePath(tileSystem, tiles, startTile, startStep, result);
        return result;
    }

    static private void AssignationChecker(TileSystem tileSystem, Tile[,] tiles)
    {
        for (int i = 0; i < tileSystem.rows * tileSystem.columns; i++)
        {
            foreach (Tile obj in tiles)
            {
                if (obj.step == i)
                {
                    TestAdjTiles(obj, obj.transform.position.y, i, tileSystem, tiles);
                }
            }
        }
    }

    static private void TestAdjTiles(Tile tile, float height, int step, TileSystem tileSystem, Tile[,] tiles)
    {
        foreach (Vector2Int adjTile in tile.adjTCoords)
        {
            if (TestAdjTile(adjTile.x, adjTile.y, height, step, tileSystem, tiles))
            {
                SetVisited(tiles[adjTile.x, adjTile.y], step);
            }
        }
    }

    static private bool TestAdjTile(int x, int y, float height, int step, TileSystem tileSystem, Tile[,] tiles)
    {
        bool cd1 = x < tileSystem.rows && x > -1;
        bool cd2 = y < tileSystem.columns && y > -1;
        bool cd3 = tiles[x, y].step == - 1;
        bool cd4 = height - tiles[x, y].transform.position.y <= tileSystem.tileP.heightByTile;
        bool cd5 = tiles[x, y].walkable;
        
        if (cd1 && cd2 && cd3 && cd4 && cd5)
        {
            return true;
        }
        else return false;
    }

    static private void SetVisited(Tile tile, int step)
    {
        if (tile)
        {
            tile.step = step + 1;
        }

    }

    static private void CreatePath(TileSystem tileSystem, Tile[,] tiles, Tile startTile, int startStep, List<Tile> result)
    {
        Tile stepTile = startTile;
        for (int i = startStep; i >= 0; i--)
        {
            foreach(Vector2Int coord in stepTile.adjTCoords)
            {
                if (tiles[coord.x, coord.y].step == i - 1)
                {
                    stepTile = tiles[coord.x, coord.y];
                    result.Add(stepTile);
                    break;
                }
            }
        }
    }
}
