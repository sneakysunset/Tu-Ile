using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.IO;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;
using UnityEditor;
using Complete;

public static class GridUtils
{
    public delegate void LevelMapLoad(string path);
    public static event LevelMapLoad onLevelMapLoad;
    public delegate void OnEndLevel(Tile tile);
    public static event OnEndLevel onEndLevel;

    #region Tile Methods
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
        
        if(ts.tiles != null && ts.tiles.GetLength(0) > x && ts.tiles.GetLength(1) > z && 0 <= x && 0 <= z) return ts.tiles[x, z];
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
                                ts[i].td.tileDegraderMult = UnityEngine.Random.Range(0.8f, 3f);
                            }
                            else if (ts[i] == tile)
                            {
                                tile.currentPos.y = 0;
                            }
                            if (!ts[i].walkable && ts[i].tourbillon)
                            {
                                ts[i].tc.tourbillonT.DOMoveY(ts[i].tc.tourbillonT.position.y - 10, 2);
                                ts[i].tourbillon = false;
                            }
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, .2f));
        }
        //yield return new WaitForSeconds(sinkTime);
        foreach (Tile t in TileSystem.Instance.tiles)
        {
            if (t.isPathChecked) t.isPathChecked = false;
            
        }
        
        tis.lerpingSpeed *= 6f;

        foreach (Interactor inte in GameObject.FindObjectsOfType<Interactor>())
        {
            ObjectPooling.SharedInstance.RemovePoolItem(0, inte.gameObject, inte.GetType().ToString() + ":" + inte.type.ToString());
        }
        string path = LoadLevelMap(toHub);
        if (onLevelMapLoad != null) onLevelMapLoad(path);
 

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

    #endregion

    #region Saving System
    public static string LoadLevelMap(bool toHub)
    {
        TileSystem tileS = TileSystem.Instance;
        RessourcesManager reMan = RessourcesManager.Instance;
        string n;
        if (toHub)
        {
            n = "TM_Hub";
            tileS.fileName = "Hub";
            TileSystem.Instance.isHub = true;
        }
        else
        {
            n = "TM_" + tileS.fileName;
            TileSystem.Instance.isHub = false;

        }
        
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
                        it.itemToSpawn = (SpawnableItems)System.Enum.Parse(typeof(SpawnableItems), tiSpawner[0]);
                        it.chosenItemToSpawn = reMan.getSpawnableFromList(tiSpawner[0]);
                        if (it.chosenItemToSpawn == null) it.enabled = false;
                        it.spawnTimer = Convert.ToInt32(tiSpawner[1]);
                        it.loop = Convert.ToBoolean(int.Parse(tiSpawner[2]));
                        it.spawnPosition = (SpawnPosition)int.Parse(tiSpawner[3]);
                        if (it.itemToSpawn == SpawnableItems.Etabli || it.itemToSpawn == SpawnableItems.Chantier) it.recette = reMan.getRecetteFromList(tiSpawner[4]);
                        else if (it.itemToSpawn == SpawnableItems.Crate || it.itemToSpawn == SpawnableItems.Mimic) it.crateReward = reMan.getRewardFromList(tiSpawner[4]);
                    }
                }
            }
        }
        //if (toHub) return tileMapInfo;
        string[] strings = tileMapInfo.Split('£');
        char c = strings[1][0];
        char c2 = strings[1][1];
        char d = strings[1][2];
        char d2 = strings[1][3];
        int x = (int)char.GetNumericValue(c) * 10 + (int)char.GetNumericValue(c2);
        int y = (int)char.GetNumericValue(d) * 10 + (int)char.GetNumericValue(d2);
        tileS.previousCenterTile = tileS.centerTile;
        tileS.centerTile = tileS.tiles[x, y];

        return tileMapInfo;
    }
    public static string GenerateMapContent()
    {
        Tile[,] t = TileSystem.Instance.tiles;
        string _content = string.Empty;
        for (int x = 0; x < t.GetLength(0); x++)
        {
            for (int y = 0; y < t.GetLength(1); y++)
            {
                _content += GetStringByTile(t[x, y]) + "\n";
            }
            _content += "|";
        }
        _content += '£';
        if (TileSystem.Instance.centerTile.coordX < 10) _content += 0 + "" + TileSystem.Instance.centerTile.coordX;
        else _content += TileSystem.Instance.centerTile.coordX;
        if (TileSystem.Instance.centerTile.coordY < 10) _content += 0 + "" + TileSystem.Instance.centerTile.coordY;
        else _content += TileSystem.Instance.centerTile.coordY;
        _content += '£';
        _content += 0;
        return _content;
    }
    public static void UpdateTileSave(string line, Tile tile, string path)
    {
        string[] content = File.ReadAllLines(path);
        int i = tile.coordX * TileSystem.Instance.rows + tile.coordY;
        content[i] = line;
        File.WriteAllLines(path, content);
    }
    public static void GenerateMap(string content, GameTimer gameManager = null)
    {
        string path = Application.streamingAssetsPath + "/LevelMaps/" + "TM_" + SceneManager.GetActiveScene().name + ".txt";
        string altPath = Application.streamingAssetsPath + "/LevelMapsSaves/" + "TM_" + SceneManager.GetActiveScene().name + ".txt";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(altPath)) File.Delete(altPath);
        File.WriteAllText(path, content);
        File.WriteAllText(altPath, content);
#if UNITY_EDITOR
        if (!TileSystem.Instance.isHub)
        {
            string gameManPath = Application.dataPath + "/Prefab/GameManagers/" + SceneManager.GetActiveScene().name + "_GM.prefab";
            GameTimer manag = PrefabUtility.SaveAsPrefabAssetAndConnect(gameManager.gameObject, gameManPath, UnityEditor.InteractionMode.UserAction).GetComponent<GameTimer>();
            RessourcesManager rMan = GameObject.FindObjectOfType<RessourcesManager>();
            if (!rMan.gameManagers.Contains(manag))
            {
                rMan.gameManagers.Add(manag);
                string rPath = Application.dataPath + "/Prefab/System/RessourcesManager.prefab";
                RessourcesManager rManag = PrefabUtility.SaveAsPrefabAssetAndConnect(rMan.gameObject, rPath, UnityEditor.InteractionMode.UserAction).GetComponent<RessourcesManager>();

            }
        }
        AssetDatabase.Refresh();
#endif
    }
    public static string GetStringByTile(Tile tile)
    {
        string result = string.Empty;
        result += tile.name;
        result += "+";
        //1
        result += "Walkable :";
        if (tile.walkable) result += 1.ToString();
        else result += 0.ToString();
        result += "+";

        //2
        result += "Degradable :";
        if (tile.degradable) result += 1.ToString();
        else result += 0.ToString();
        result += "+";

        //3
        result += "Tourbillon :";
        if (tile.tourbillon) result += 1.ToString();
        else result += 0.ToString();
        result += "+";

        //4
        result += "TileType :";
        result += Convert.ToInt32(tile.tileType);
        result += "+";

        //5
        result += "TileSpawnType :";
        result += Convert.ToInt32(tile.tileSpawnType);
        result += "+";

        //6
        result += "SpawnPosition :";
        result += Convert.ToString((int)tile.spawnPositions);
        result += "+";


        //7
        result += "LevelName :";
        result += tile.levelName;
        if (tile.levelName.Length == 0) result += "NIVEAU VIDE";
        result += "+";

        //8
        result += "Height :";
        if(Application.isPlaying) result += tile.currentPos.y;
        else result += tile.transform.position.y;
        result += "+";

        //9
        result += "ItemSpawner :";
        if (tile.TryGetComponent<ItemSpawner>(out ItemSpawner itemSpawner))
        {
            string itemType = itemSpawner.itemToSpawn.ToString();
            result += itemType;
            result += ";";
            result += itemSpawner.spawnTimer.ToString();
            result += ";";
            result += Convert.ToInt32(itemSpawner.loop);
            result += ";";
            result += Convert.ToInt32(itemSpawner.spawnPosition);
            result += ";";
            if (itemSpawner.itemToSpawn == SpawnableItems.Chantier) result += itemSpawner.otherRecette;
            else if (itemSpawner.itemToSpawn == SpawnableItems.Etabli) result += itemSpawner.recette.ToString();
            else if (itemSpawner.itemToSpawn == SpawnableItems.Crate || itemSpawner.itemToSpawn == SpawnableItems.Mimic) result += itemSpawner.crateReward.ToString();
        }
        else
        {
            result += "No Spawner";
        }
        result += "%";

        return result;
    }

    public static void ResetGame()
    {
        string path = Application.streamingAssetsPath + "/LevelMaps";
        string altPath = Application.streamingAssetsPath + "/LevelMapsSaves";
        FileInfo[] fileInfo = new DirectoryInfo(path).GetFiles();
        FileInfo[] altFileInfo = new DirectoryInfo(altPath).GetFiles();
        for (int i = 0; i < altFileInfo.Length; i++)
        {
            altFileInfo[i].CopyTo(fileInfo[i].FullName, true);
        }
    }

#endregion
}
