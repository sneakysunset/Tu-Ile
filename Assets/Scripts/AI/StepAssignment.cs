using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class StepAssignment 
{
    static public List<Tile> Initialisation(Tile targetTile, Tile startTile, int range)
    {
        List<Tile> result = new List<Tile>();
        //Tile[,] tiles = tileSystem.tiles;
        List<Tile> tiles = GridUtils.GetTilesAround(range, startTile);
        foreach (Tile obj in tiles)
        {
            obj.step = -1;
            if (!obj.walkable)
            {
                obj.step = -2;
            }
        }
        targetTile.step = 0;
        AssignationChecker(tiles, range + 1);
        
        int startStep = startTile.step;
        CreatePath(startTile, startStep, result);
        return result;
    }

    static private void AssignationChecker( /*Tile[,] tiles*/ List<Tile> tiles, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            foreach (Tile obj in tiles)
            {
                if (obj.step == i)
                {
                    TestAdjTiles(obj, obj.transform.position.y, i, tiles);
                }
            }
        }
    }

    static private void TestAdjTiles(Tile tile, float height, int step, /*Tile[,] tiles*/ List<Tile> tiles)
    {
        foreach (Vector2Int adjTile in tile.adjTCoords)
        {
            if (TestAdjTile(adjTile.x, adjTile.y, height, step, tiles))
            {
                SetVisited(TileSystem.Instance.tiles[adjTile.x, adjTile.y], step);
            }
        }
    }

    static private bool TestAdjTile(int x, int y, float height, int step, /*Tile[,] tiles*/ List<Tile> tiles)
    {
        bool cd1 = x < TileSystem.Instance.rows && x > -1;
        bool cd2 = y < TileSystem.Instance.columns && y > -1;
        if (cd1 || cd2) return false;
        bool cd3 = TileSystem.Instance.tiles[x, y].step == - 1;
        bool cd6 = tiles.Contains(TileSystem.Instance.tiles[x, y]);
        bool cd4 = height - TileSystem.Instance.tiles[x, y].transform.position.y <= TileSystem.Instance.centerTile.td.heightByTile;
        bool cd5 = TileSystem.Instance.tiles[x, y].walkable;
        
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

    static private void CreatePath(Tile startTile, int startStep, List<Tile> result)
    {
        Tile stepTile = startTile;
        for (int i = startStep; i >= 0; i--)
        {
            foreach(Vector2Int coord in stepTile.adjTCoords)
            {
                if (TileSystem.Instance.tiles[coord.x, coord.y].step == i - 1)
                {
                    stepTile = TileSystem.Instance.tiles[coord.x, coord.y];
                    result.Add(stepTile);
                    break;
                }
            }
        }
    }
}
