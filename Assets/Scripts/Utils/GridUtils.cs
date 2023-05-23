using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.SceneManagement;

public static class GridUtils
{
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
    public static Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = TileSystem.Instance.tiles[x, z].transform.localScale.x * .85f;
        Vector3 pos = ogPos + new Vector3(x * TileSystem.Instance.tiles[x, z].transform.localScale.x * 1.7f + xOffset, 0, z * TileSystem.Instance.tiles[x, z].transform.localScale.z * 1.48f);
        TileSystem.Instance.tiles[x, z].coordX = x;

        TileSystem.Instance.tiles[x, z].coordY = z;
        return pos;
    }

    public static IEnumerator SinkWorld(Tile _centerTile, string levelToLoad, bool isEnd)
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
                t.tourbillon = false;
                t.tourbillonT.gameObject.SetActive(false);
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
        SceneManager.LoadScene(levelToLoad, LoadSceneMode.Single);
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
