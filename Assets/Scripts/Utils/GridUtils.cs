using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.IO;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;
using static UnityEditor.Progress;

public static class GridUtils
{
    public delegate void LevelMapLoad();
    public static event LevelMapLoad onLevelMapLoad;
    public delegate void OnEndLevel(Tile tile);
    public static event OnEndLevel onEndLevel;

    public static void LoadLevelMap(bool toHub)
    {
        TileSystem tileS = TileSystem.Instance;
        RessourcesManager reMan = RessourcesManager.Instance;
        string n;
        if (toHub)
        {
            n = "TM_Hub";
            tileS.fileName = "Hub";
        }
        else n = "TM_" + tileS.fileName;
        
        string tileMapInfo = File.ReadAllText(Application.streamingAssetsPath + "/LevelMaps/" + n + ".txt");
        string[] tiLine = tileMapInfo.Split('|');
        for (int i = 0; i < tiLine.Length - 1; i++)
        {

                if (i >= TileSystem.Instance.rows) break;
            string[] tiRow = tiLine[i].Split('%');
            for (int k = 0; k < tiRow.Length - 1; k++)
            {
                if (k >= TileSystem.Instance.rows) continue;
                //Debug.Log(i + " " + k);
                string[] tiParam = tiRow[k].Split('+');

                Tile tile = tileS.tiles[i, k];

                if (tile.TryGetComponent(out ItemSpawner itemSpawner)) GameObject.Destroy(itemSpawner);
                tile.walkable = Convert.ToBoolean(int.Parse(tiParam[1].Split(":")[1]));
                tile.tourbillon = Convert.ToBoolean(int.Parse(tiParam[3].Split(":")[1]));
                tile.tileSpawnType = (TileType)Convert.ToInt32(int.Parse(tiParam[5].Split(":")[1]));
                if (tile.walkable)
                {
                    
                    tile.degradable = Convert.ToBoolean(int.Parse(tiParam[2].Split(":")[1]));
                    tile.tileType = (TileType)Convert.ToInt32(int.Parse(tiParam[4].Split(":")[1]));
                    tile.spawnPositions = (SpawnPositions)int.Parse(tiParam[6].Split(":")[1]);
                    tile.levelName = tiParam[7].Split(":")[1];
                    tile.currentPos.y = int.Parse(tiParam[8].Split(":")[1]);
                    string[] tiSpawner = tiParam[9].Split(":")[1].Split(";");
                    if (tiSpawner.Length > 1)
                    {
                        ItemSpawner it = tile.AddComponent<ItemSpawner>();
                        it.chosenItemToSpawn = reMan.getSpawnableFromList(tiSpawner[0]);
                        if(it.chosenItemToSpawn == null) it.enabled = false;
                        it.spawnTimer = Convert.ToInt32(tiSpawner[1]);
                        it.loop = Convert.ToBoolean(int.Parse(tiSpawner[2]));
                        it.spawnPosition = (SpawnPosition)int.Parse(tiSpawner[3]);
                        if (it.itemToSpawn == SpawnableItems.Etabli || it.itemToSpawn == SpawnableItems.Chantier) it.recette = reMan.getRecetteFromList(tiSpawner[4]);
                    }
                }
            }
        }
        if (toHub) return;
        string[] strings = tileMapInfo.Split('£');
        char c = strings[1][0];
        char c2 = strings[1][1];
        char d = strings[1][2];
        char d2 = strings[1][3];
        int x = (int)char.GetNumericValue(c) * 10 + (int)char.GetNumericValue(c2);
        int y = (int)char.GetNumericValue(d) * 10 + (int)char.GetNumericValue(d2);
        tileS.centerTile = tileS.tiles[x, y];
    }

    public static Tile WorldPosToTile(Vector3 pos)
    {
        TileSystem ts;
        if(TileSystem.Instance != null)ts = TileSystem.Instance;
        else ts = GameObject.FindObjectOfType<TileSystem>();
        float xOffset = 0;
        int x;
        int z;


        z = Mathf.RoundToInt(pos.z / (ts.tilePrefab.transform.localScale.z * 1.48f));
        if (z % 2 == 1) xOffset = ts.tilePrefab.transform.localScale.x * .9f;
        x = Mathf.RoundToInt((pos.x - xOffset) / (ts.tilePrefab.transform.localScale.x * 1.7f));
        
        if(ts.tiles != null && ts.tiles.Length > x && ts.tiles.LongLength > z) return ts.tiles[x, z];
        else return null;
    }

    public static Vector3 indexToWorldPos(int x, int z, Vector3 ogPos, Transform tT)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = tT.localScale.x * .85f;
        Vector3 pos = ogPos + new Vector3(x * tT.localScale.x * 1.7f + xOffset, 0, z * tT.localScale.z * 1.48f);

        return pos;
    }

    public static IEnumerator SinkWorld(Tile _centerTile, bool isEnd, bool toHub)
    {
        TileSystem tis = TileSystem.Instance;
        Tile tile = _centerTile;
        if (onEndLevel != null) onEndLevel(tile);

        List<Tile> ts = new List<Tile> { tile };

        tis.lerpingSpeed /= 6f;
        
        bool isOver = false;

        while (!isOver)
        {
            isOver = true;
            int ix = ts.Count;
            //ts.Sort((a, b) => 1 - 2 * UnityEngine.Random.Range(0, 2));
            for (int i = 0; i < ix; i++)
            {
                if (!ts[i].isPathChecked)
                {
                    foreach (Vector2Int vecs in ts[i].adjTCoords)
                    {
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns /*&& tiles[vecs.x, vecs.y].walkable*/ && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tis.tiles[vecs.x, vecs.y]);
                            isOver = false;
                            if (ts[i].walkable && ts[i] != tile)
                            {
                                ts[i].degradable = true;
                                ts[i].currentPos.y = -16;
                            }
                            else if (ts[i] == tile)
                            {
                                tile.currentPos.y = 0;
                            }
                            if (!ts[i].walkable && ts[i].tourbillon)
                            {
                                ts[i].tourbillonT.DOMoveY(ts[i].tourbillonT.position.y - 10, 2);
                                ts[i].tourbillon = false;
                            }
                        }
                        //yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .02f));
                    }
                    ts[i].isPathChecked = true;
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .2f));
        }
        yield return new WaitForSeconds(2);
        
        tis.lerpingSpeed *= 6f;

        foreach (Interactor inte in GameObject.FindObjectsOfType<Interactor>())
        {
            //GameObject.Destroy(inte.gameObject);
            ObjectPooling.SharedInstance.RemovePoolItem(0, inte.gameObject, inte.GetType().ToString() + ":" + inte.type.ToString());

        }
        LoadLevelMap(toHub);
        if (onLevelMapLoad != null) onLevelMapLoad();
        //GameObject.Destroy(PlayersManager.Instance.gameObject);
        //GameObject.Instantiate(obj);
        //tis.ready = false;
    }


/*
    public static IEnumerator ElevateWorld(Tile _centerTile)
    {
        TileSystem tis = TileSystem.Instance;
        Tile tile;
        if (tis.isHub) tile = _centerTile;
        else tile = tis.centerTile;
        if (!tis.isHub)
            gT.UIPos.anchoredPosition += Vector2.up * -100;
        if (!tis.isHub)
            tis.StartCoroutine(gT.LerpTimeLine(gT.UIPos.anchoredPosition, gT.UIPos.anchoredPosition + Vector2.up * 100, gT.UIPos, gT.lerpCurveEaseOut, gT.lerpSpeed));
        List<Tile> ts = new List<Tile>();
        ts.Add(tile);
        bool isOver = false;
        int j = 0;
        while (!isOver)
        {
            isOver = true;
            int ix = ts.Count;
            for (int i = 0; i < ix; i++)
            {
                if (!ts[i].isPathChecked)
                {
                    foreach (Vector2Int vecs in ts[i].adjTCoords)
                    {
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns && tiles[vecs.x, vecs.y].walkable && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tis.tiles[vecs.x, vecs.y]);
                            isOver = false;
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }


            foreach (Tile t in ts)
            {
                if (t.walkable)
                {
                    t.readyToRoll = true;
                }
            }
            tile.degradable = false;
            j++;
            float timer = tis.waitTimer / j;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                foreach (Tile t in ts)
                {
                    TileSystem.Instance.lerpingSpeed /= 2.5f;
                    if (t.walkable && t != tile)
                    {

                        //t.degSpeed *= 1 + Time.deltaTime * 3 * Random.Range(0f,1f);
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        TileSystem.Instance.lerpingSpeed = 1;
        tis.ready = true;



        foreach (Tile t in ts)
        {
            t.isPathChecked = false;
        }
    }
*/


    public static List<Tile> GetTilesAround(int numOfRows, Tile tile)
    {
        TileSystem tis = TileSystem.Instance;
        List<Tile> ts = new List<Tile>();
        int rowsSeen = 0;
        ts.Add(tile);
        while (rowsSeen < numOfRows)
        {
            int ix = ts.Count;
            for (int i = 0; i < ix; i++)
            {
                if (!ts[i].isPathChecked)
                {
                    foreach (Vector2Int vecs in ts[i].adjTCoords)
                    {
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns && tis.tiles[vecs.x, vecs.y].walkable && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tis.tiles[vecs.x, vecs.y]);
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }
            rowsSeen++;
        }
        //ts.Remove(tile);
        foreach (Tile t in ts)
        {
            t.isPathChecked = false;
        }
        return ts;
    }

    public static List<Tile> GetTilesBetweenRaws(int rowMin, int rowMax, Tile tile)
    {
        TileSystem tis = TileSystem.Instance;
        List<Tile> ts = new List<Tile>();
        List<Tile> ts2 = new List<Tile>();
        int rowsSeen = 0;
        ts.Add(tile);
        while (rowsSeen <= rowMax)
        {
            int ix = ts.Count;
            for (int i = 0; i < ix; i++)
            {
                if (!ts[i].isPathChecked)
                {
                    foreach (Vector2Int vecs in ts[i].adjTCoords)
                    {
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tis.tiles[vecs.x, vecs.y]);
                            if (rowsSeen >= rowMin && rowsSeen <= rowMax)
                            {
                                ts2.Add(tis.tiles[vecs.x, vecs.y]);
                            }
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }
            rowsSeen++;
        }

        foreach (Tile t in ts)
        {
            t.isPathChecked = false;
        }
        return ts2;
    }
}
