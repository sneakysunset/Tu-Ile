using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
using Unity.VisualScripting.FullSerializer;
using static UnityEngine.Rendering.DebugUI.Table;
#if UNITY_EDITOR
using AmplifyShaderEditor;
#endif
public class TileSystem : MonoBehaviour
{
    public bool InstantiateGrid;
    public bool DestroyGrid;
    public bool UpdateParameters;
    public bool GenerateMap;
    public int columns, rows;
    public string fileName;
    [HideInInspector, SerializeField] public Tile[,] tiles;
    public Tile tilePrefab;
    [HideInInspector] public TileParameters tileP;
    [HideInInspector] public TileMats tileM;
    private GameTimer gT;
    [HideInInspector] public Vector3 gridOgTile;
    static bool editorFlag = false;
    public Tile centerTile;
    [HideInInspector] public Transform tileFolder;
    [HideInInspector] public TileCounter tileC;
    public bool isHub;
    public static TileSystem Instance { get; private set; }
    [HideNormalInspector] public float degradationTimerModifier;
    float timerInterpolateValue;
    [HideInInspector] public bool ready = true;
    [HideNormalInspector] public float lerpingSpeed = 1;
    [HideInInspector] public float waitTimer = .3f;
    [HideNormalInspector] public Tile previousCenterTile;
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }


        if (this.enabled)
        {
            RegenGrid();
        }

        tileC = GetComponent<TileCounter>();
        GridUtils.onLevelMapLoad += OnLoadScene;
    }

    private void Start()
    {
        if (!isHub)
        {
            GameTimer gt = FindObjectOfType<GameTimer>();
        }
    }

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLoadScene;
        editorFlag = true;
    }

    private void OnLoadScene()
    {
        if(gT == null) gT = FindObjectOfType<GameTimer>();
        if (fileName != "Hub") isHub = false;
        else isHub = true;

        //StartCoroutine(ElevateWorld());
        Vector2Int vector2Int = FindObjectOfType<CameraCtr>().tileLoadCoordinates;
        if (!isHub)
        {
            previousCenterTile.transform.GetChild(9).gameObject.SetActive(false);
            centerTile.transform.GetChild(9).gameObject.SetActive(true);
        }
        //StartCoroutine(GridUtils.ElevateWorld(tiles[vector2Int.x, vector2Int.y]));
    }


    IEnumerator ElevateWorld()
    {
        yield return new WaitForEndOfFrame();
    }

    private void Update()
    {
        if (!isHub)
        {
            timerInterpolateValue += Time.deltaTime * (1 / gT.gameTimer);
            degradationTimerModifier = tileP.degradationTimerAnimCurve.Evaluate(timerInterpolateValue); 
        } 
    }

    void RegenGrid()
    {
        Tile[] list = FindObjectsOfType<Tile>();
        if (list.Length == 0) return;


        tiles = new Tile[rows, columns];
        for (int i = 0; i < list.Length; i++)
        {
            int x = list[i].coordX;
            int y = list[i].coordY;
            tiles[x, y] = list[i];
            tiles[x, y].name = "tile " + x + " " + y;
        }
        UpdateGridParameters();
    }

    public void UpdateGridParameters()
    {
        tileM = GetComponent<TileMats>();
        tileP = GetComponent<TileParameters>();

        foreach (var tile in tiles)
        {
            tile.degradationTimerMin = tileP.degradationTileTimerMin;
            tile.degradationTimerMax = tileP.degradationTileTimerMax;
            tile.degradationTimerAnimCurve = tileP.degradationTimerAnimCurve;
            tile.heightByTile = tileP.heightByTile;
            //tile.falaiseMat = tileM.falaiseTileMat;
            tile.plaineMatBottom = tileM.plaineTileMatBottom;
            tile.plaineMatTop = tileM.plaineTileMatTop;
            tile.disabledMat = tileM.disabledTileMaterial;
            tile.sandMatTop = tileM.desertTileMatTop;
            tile.sandMatBottom = tileM.desertTileMatBottom;
            tile.bounceMat = tileM.bounceTileMat;
            tile.undegradableMat = tileM.undegradableTileMat;
            tile.undegradableMatBottom = tileM.undegradableTileMatBottom;
            tile.woodMat = tileM.woodTileMat;
            tile.rockMat = tileM.rockTileMat;
            tile.goldMat = tileM.goldTileMat;
            tile.diamondMat = tileM.diamondTileMat;
            tile.adamantiumMat = tileM.adamantiumTileMat;
            tile.centerTileMat = tileM.centerTileMat;
            tile.centerTileMatBottom = tileM.centerTileMatBottom;
            tile.defaultMesh = tileM.defaultTileMesh;
            tile.colliderMesh = tileM.meshCollider;
            tile.woodMesh = tileM.woodTileMesh;
            tile.rockMesh = tileM.rockTileMesh;
            tile.sandMesh = tileM.sandTileMesh;
            tile.undegradableMesh = tileM.undegradableTileMesh;
            tile.centerTileMesh = tileM.centerTileMesh;
            tile.notWalkedOnColor = tileM.notWalkedOnColor;
            tile.walkedOnColor = tileM.walkedOnColor;
            tile.penguinedColor = tileM.acceleratedDegradationColor;
        }
    }

    private void OnDrawGizmos()
    {
        if((editorFlag || tiles == null) && !Application.isPlaying)
        {
            editorFlag = false;
            RegenGrid();
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(TileSystem))]
[System.Serializable]
public class TileSystemEditor : Editor
{
    public TileSystem tileS;
    private string _content;
    private void OnEnable()
    {
        tileS = (TileSystem)target;
        tileS.UpdateGridParameters();
        foreach (var tile in tileS.tiles)
        {
            tile.UpdateObject();
        }
    }


    private void OnSceneGUI()
    {
        if (!Application.isPlaying)
        {
            Draw();
            EditorUtility.SetDirty(tileS);
        }
    }

    void Draw()
    {
        base.OnInspectorGUI();

        if (tileS.GenerateMap)
        {
            tileS.GenerateMap = false;
            GenerateMapContent(tileS.tiles);
            GenerateMap(_content);
        }

        if (tileS.InstantiateGrid)
        {
            InstantiateGrid();
            //UpdateGridParameters();
        }

        if (tileS.DestroyGrid)
        {
            DestroyGrid();
        }

        if (tileS.UpdateParameters)
        {
            UpdateGridParameters();
        }
    }

    void GenerateMapContent(Tile[,] t)
    {
        _content = string.Empty;
        for (int x = 0; x < t.GetLength(0); x++)
        {
            for (int y = 0; y < t.GetLength(1); y++)
            {
                //0
                if (t[x, y].walkable) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";
                
                //1
                if (t[x, y].degradable) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";                
                
                //2
                if (t[x, y].tourbillon) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";

                //3
                _content += Convert.ToInt32(t[x, y].tileType);
                /*switch(t[x, y].tileType)
                {
                    case TileType.Neutral: _content += 0.ToString(); break;
                    case TileType.Wood: _content += 1.ToString(); break;
                    case TileType.Rock: _content += 2.ToString(); break;
                    case TileType.Gold: _content += 3.ToString(); break;
                    case TileType.Diamond: _content += 4.ToString(); break;
                    case TileType.Adamantium: _content += 5.ToString(); break;
                    case TileType.Sand: _content += 6.ToString(); break;
                    case TileType.BouncyTile: _content += 7.ToString(); break;
                    case TileType.LevelLoader: _content += 8.ToString(); break;
                    case TileType.construction: _content += 9.ToString(); break;
                }*/
                _content += "+";

                //4
                _content += Convert.ToInt32(t[x, y].tileSpawnType);
                /*switch (t[x, y].tileSpawnType)
                {
                    case TileType.Neutral: _content += 0.ToString(); break;
                    case TileType.Wood: _content += 1.ToString(); break;
                    case TileType.Rock: _content += 2.ToString(); break;
                    case TileType.Gold: _content += 3.ToString(); break;
                    case TileType.Diamond: _content += 4.ToString(); break;
                    case TileType.Adamantium: _content += 5.ToString(); break;
                    case TileType.Sand: _content += 6.ToString(); break;
                    case TileType.BouncyTile: _content += 7.ToString(); break;
                    case TileType.LevelLoader: _content += 8.ToString(); break;
                    case TileType.construction: _content += 9.ToString(); break;
                }*/
                _content += "+";

                //5
                _content += Convert.ToString((int)t[x, y].spawnPositions);
                _content += "+";
/*                int myInt = Convert.ToInt32(t[x, y].spawnPositions);
*//*                bool[] bools = Utils.GetSpawnPositions(myInt);
                for (int i = 0; i < bools.Length; i++)
                {
                    if (bools[i]) _content += 1;
                    else _content += 0;
                    if(i < bools.Length - 1)_content += ";";
                }*//*
                string flag = ; ;*/


                //6
                _content += t[x, y].levelName;
                if (t[x, y].levelName.Length == 0) _content += "NIVEAU VIDE";
                _content += "+";

                //7
                _content += t[x, y].transform.position.y;
                _content += "+";

                if (t[x,y].TryGetComponent<ItemSpawner>(out ItemSpawner itemSpawner))
                {
                    string itemType = itemSpawner.itemToSpawn.ToString();
                    _content += itemType;
                    _content += ";";
                    _content += itemSpawner.spawnTimer.ToString();
                    _content += ";";
                    _content += Convert.ToInt32(itemSpawner.loop);                    
                    _content += ";";
                    _content += Convert.ToInt32(itemSpawner.spawnPosition);
                }
                else
                {
                    _content += "No Spawner";
                }

                _content += "-";
            }
            _content += "|";
        }
        _content += '(';
        if (tileS.centerTile.coordX < 10) _content += 0 + "" + tileS.centerTile.coordX;
        else _content += tileS.centerTile.coordX;
        if (tileS.centerTile.coordY < 10) _content += 0 + "" + tileS.centerTile.coordY;
        else _content += tileS.centerTile.coordY;

    }

    void GenerateMap(string content)
    {
        string path = Application.dataPath + "/LevelMaps/" + "TM_" + SceneManager.GetActiveScene().name + ".txt";
        
        File.Delete(path);
        File.WriteAllText(path, content);
        AssetDatabase.Refresh();
    }



    void InstantiateGrid()
    {
        tileS.InstantiateGrid = false;
        Transform tileFolder = GameObject.FindGameObjectWithTag("TileFolder").transform;

        Tile[] list = FindObjectsOfType<Tile>();
        if (list.Length == 0) return;

        int o = (int)Mathf.Sqrt(list.Length);
        tileS.tiles = new Tile[o, o];
        for (int i = 0; i < list.Length; i++)
        {
            int x = list[i].coordX;
            int y = list[i].coordY;
            tileS.tiles[x, y] = list[i];
            tileS.tiles[x, y].name = "tile " + x + " " + y;
        }

        if (tileS.tiles == null) tileS.tiles = new Tile[tileS.rows, tileS.columns]; ;
        if (tileS.tiles[0, 0] == null)
        {
            tileS.tiles = new Tile[tileS.rows, tileS.columns];
            for (int i = 0; i < tileS.rows; i++)
            {
                for (int j = 0; j < tileS.columns; j++)
                {

                    Tile tile = PrefabUtility.InstantiatePrefab(tileS.tilePrefab) as Tile;
                    tile.transform.parent = tileFolder.transform;
                    tileS.tiles[i, j] = tile;
                    tile.transform.position = GridUtils.indexToWorldPos(i, j, tileS.gridOgTile, tileS.tilePrefab.transform);
                    tile.gameObject.name = i + "  " + j;

                }
            }
        }
        else
        {
            Tile[,] tempTiles = new Tile[tileS.rows, tileS.columns];

            int maxX = tileS.tiles.GetLength(0);
            int maxY = tileS.tiles.GetLength(1);
            if (tileS.rows > tileS.tiles.GetLength(0)) maxX = tileS.rows;
            if (tileS.columns > tileS.tiles.GetLength(1)) maxY = tileS.columns;
            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxY; j++)
                {
                    if (i >= tileS.tiles.GetLength(0) || j >= tileS.tiles.GetLength(1) )
                    {
                        Tile tile = PrefabUtility.InstantiatePrefab(tileS.tilePrefab) as Tile;
                        tile.transform.parent = tileS.tileFolder;
                        tempTiles[i, j] = tile;
                        tile.transform.position = GridUtils.indexToWorldPos(i, j, tileS.gridOgTile, tileS.tilePrefab.transform);
                        tile.gameObject.name = i + "  " + j;
                        tile.coordX = i;
                        tile.coordY = j;
                        tile.walkable = false;
                    }
                    else if (i >= tileS.rows || j >= tileS.columns)
                    {
                        Debug.Log(i+ " " +  j);
                        DestroyImmediate(tileS.tiles[i, j].gameObject);
                    }
                    else if(i < tileS.tiles.GetLength(0) && j < tileS.tiles.GetLength(1) && i < tileS.rows && j < tileS.columns && tileS.tiles[i, j] != null)
                    {
                        tempTiles[i, j] = tileS.tiles[i, j];
                    }  
                }


            }
            //tileS.tiles = new Tile[tileS.rows, tileS.columns];
            tileS.tiles = tempTiles;
/*            foreach (Tile tile in tempTiles)
            {
                Debug.Log(tile.name);
            }*/
        }
    }

    void UpdateGridParameters()
    {

        tileS.UpdateParameters = false;
        if (tileS.tiles == null) return;
        tileS.UpdateGridParameters();
    }

    void DestroyGrid()
    {
        tileS.DestroyGrid = false;
        if(tileS.tiles != null)
        {
            for (int i = 0; i < tileS.tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tileS.tiles.GetLength(1); j++)
                {
                    if(tileS.tiles[i, j])
                    DestroyImmediate(tileS.tiles[i, j].gameObject);
                }
            }
            tileS.tiles = null;
        }
        if(tileS.transform.childCount > 0)
        {
            int max = tileS.transform.childCount;
            for (int i = max - 1; i >= 0; i--)
            {
                DestroyImmediate(tileS.transform.GetChild(i).gameObject);
            }
        }
    }
}
#endif