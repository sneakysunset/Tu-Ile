using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.IO;
using UnityEngine.Events;
public static class GridUtils
{
    public delegate void LevelMapLoad();
    public static event LevelMapLoad onLevelMapLoad;

    public static void LoadLevelMap(bool toHub)
    {
        TileSystem tileS = TileSystem.Instance;
        string n;
        if (toHub) n = "TM_Hub";
        else n = "TM_" + tileS.fileName;
        string tileMapInfo = File.ReadAllText(Application.dataPath + "/LevelMaps/" + n + ".txt");
        string[] tiLine = tileMapInfo.Split('|');
        Debug.Log(tiLine.Length);
        for (int k = 0; k < tiLine.Length - 1; k++)
        {

            string[] tiRow = tiLine[k].Split('-');
            for (int i = 0; i < tiRow.Length - 1; i++)
            {
                string[] tiParam = tiRow[i].Split('+');

                if (tiParam[0] == "1") tileS.tiles[i, k].walkable = true;
                else tileS.tiles[i, k].walkable = false;

                if (tileS.tiles[i, k].walkable)
                {
                    if (tiParam[1] == "1") tileS.tiles[i, k].degradable = true;
                    else tileS.tiles[i, k].degradable = false;

                    if (tiParam[2] == "1") tileS.tiles[i, k].tourbillon = true;
                    else tileS.tiles[i, k].tourbillon = false;

                    switch (tiParam[3])
                    {
                        case "0": tileS.tiles[i, k].tileType = TileType.Neutral; break;
                        case "1": tileS.tiles[i, k].tileType = TileType.Wood; break;
                        case "2": tileS.tiles[i, k].tileType = TileType.Rock; break;
                        case "3": tileS.tiles[i, k].tileType = TileType.Gold; break;
                        case "4": tileS.tiles[i, k].tileType = TileType.Diamond; break;
                        case "5": tileS.tiles[i, k].tileType = TileType.Adamantium; break;
                        case "6": tileS.tiles[i, k].tileType = TileType.Sand; break;
                        case "7": tileS.tiles[i, k].tileType = TileType.BouncyTile; break;
                        case "8": tileS.tiles[i, k].tileType = TileType.LevelLoader; break;
                        case "9": tileS.tiles[i, k].tileType = TileType.construction; break;
                    }

                    /*string[] tisPos = tiRow[5].Split(';');

                    int num = 0;
                    int mult = 1;
                    for (int v = 0; v < tisPos.Length; v++)
                    {
                        if (tisPos[v] == "1") num += mult;
                        mult *= 2;
                    }
                    tileS.tiles[i, k].spawnPositions.;*/

                    //Debug.Log(int.Parse(tiRow[7][0]));
                    tileS.tiles[i, k].transform.position = new Vector3(tileS.tiles[i, k].transform.position.x, int.Parse(tiParam[7]), tileS.tiles[i, k].transform.position.z);
                }
                switch (tiParam[4])
                {
                    case "0": tileS.tiles[i, k].tileSpawnType = TileType.Neutral; break;
                    case "1": tileS.tiles[i, k].tileSpawnType = TileType.Wood; break;
                    case "2": tileS.tiles[i, k].tileSpawnType = TileType.Rock; break;
                    case "3": tileS.tiles[i, k].tileSpawnType = TileType.Gold; break;
                    case "4": tileS.tiles[i, k].tileSpawnType = TileType.Diamond; break;
                    case "5": tileS.tiles[i, k].tileSpawnType = TileType.Adamantium; break;
                    case "6": tileS.tiles[i, k].tileSpawnType = TileType.Sand; break;
                    case "7": tileS.tiles[i, k].tileSpawnType = TileType.BouncyTile; break;
                    case "8": tileS.tiles[i, k].tileSpawnType = TileType.LevelLoader; break;
                    case "9": tileS.tiles[i, k].tileSpawnType = TileType.construction; break;
                }

                tileS.tiles[i, k].levelName = tiRow[6];

            }
        }
        string[] strings = tileMapInfo.Split('(');
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
        float xOffset = 0;
        int x;
        int z;


        z = Mathf.RoundToInt(pos.z / (TileSystem.Instance.tilePrefab.transform.localScale.z * 1.48f));
        if (z % 2 == 1) xOffset = TileSystem.Instance.tilePrefab.transform.localScale.x * .9f;
        x = Mathf.RoundToInt((pos.x - xOffset) / (TileSystem.Instance.tilePrefab.transform.localScale.x * 1.7f));

        if(TileSystem.Instance.tiles.Length > x && TileSystem.Instance.tiles.LongLength > z) return TileSystem.Instance.tiles[x, z];
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
        GameTimer gT = GameObject.FindObjectOfType<GameTimer>();
        Tile tile = _centerTile;
        List<Tile> ts = new List<Tile>();
        ts.Add(tile);
        bool isOver = false;
        tis.lerpingSpeed = .1f;
        int j = 0;
        tis.ready = false;
        foreach (var item in GameObject.FindObjectsOfType<Player>())
        {
            item.respawnTile = tile;
        }
        if (!tis.isHub && !isEnd)
        {
            tis.StartCoroutine(gT.LerpTimeLine(gT.UIPos.anchoredPosition, gT.UIPos.anchoredPosition + Vector2.up * -100, gT.UIPos, gT.lerpCurveEaseIn, gT.lerpSpeed));
            MissionManager.Instance.CloseMissions();
        }

        GameObject.FindObjectOfType<CameraCtr>().DezoomCam(tile.transform.GetChild(0));
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
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns /*&& tiles[vecs.x, vecs.y].walkable*/ && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
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
                if (t.walkable && t != tile)
                {
                    t.degradable = true;
                    t.currentPos.y = -t.heightByTile;
                }
                else if (t == tile)
                {
                    tile.currentPos.y = 0;
                    t.degSpeed *= 2;
                }
                if(!t.walkable && t.tourbillon) t.tourbillonT.DOMoveY(t.tourbillonT.position.y - 10, 2);
            }
            tile.degradable = false;

            j++;
            float timer = tis.waitTimer / j;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                foreach (Tile t in ts)
                {
                    if (t.walkable && t != tile)
                    {
                        t.degSpeed *= 1 + Time.deltaTime * 3;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }


        yield return new WaitForSeconds(2);
        //SceneManager.LoadScene(levelToLoad, LoadSceneMode.Single);
        LoadLevelMap(toHub);
        if (onLevelMapLoad != null) onLevelMapLoad();
        //GameObject.Destroy(PlayersManager.Instance.gameObject);
        //GameObject obj = Resources.Load("") as GameObject;
        //GameObject.Instantiate(obj);
        tis.ready = false;
    }

    public static IEnumerator ElevateWorld(Tile _centerTile)
    {
        TileSystem tis = TileSystem.Instance;
        GameTimer gT = GameObject.FindObjectOfType<GameTimer>();
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
                        if (vecs.x >= 0 && vecs.x < tis.rows && vecs.y >= 0 && vecs.y < tis.columns /*&& tiles[vecs.x, vecs.y].walkable*/ && !ts.Contains(tis.tiles[vecs.x, vecs.y]))
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
