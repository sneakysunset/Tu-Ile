using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
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
    [HideInInspector] public Tile[,] tiles;
    public Tile tilePrefab;
    [HideInInspector] public TileParameters tileP;
    [HideInInspector] public TileMats tileM;
    [HideInInspector] public TileMovements tileMov;
    [HideInInspector] public Vector3 gridOgTile;
    static bool editorFlag = false;
    public Tile centerTile;
    [HideInInspector] public Transform tileFolder;
    public bool isHub;
    public static TileSystem Instance { get;  set; }
    [HideNormalInspector] public float degradationTimerModifier;
    float timerInterpolateValue;
    [HideInInspector] public bool ready = true;
    [HideNormalInspector] public float lerpingSpeed = 1;
    [HideNormalInspector] public Tile previousCenterTile;
    [HideInInspector] public WaitForSeconds shakeWaiter;
    [HideInInspector] public WaitForSeconds shakeLongWaiter;

    [HideInInspector] public CameraCtr cam;
    [HideInInspector] public PlayersManager playersMan;
    [HideInInspector] public TutorialManager tutorial;
    [HideInInspector] public TileCounter tileCounter;
    [HideInInspector] public GameTimer gameTimer;
    [HideInInspector] public ScoreManager scoreManager;
    [HideInInspector] public CompassMissionManager compassManager;
    public AnimationCurve easeIn, easeOut, easeInOut;
    public RessourcesManager ressourcesManagerPrefab;
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

        cam = FindObjectOfType<CameraCtr>();
        playersMan = FindObjectOfType<PlayersManager>();
        tutorial = playersMan.GetComponent<TutorialManager>();
        if (!isHub)
        {
            gameTimer = FindObjectOfType<GameTimer>();
            compassManager = gameTimer.GetComponent<CompassMissionManager>();
            tileCounter = gameTimer.GetComponent<TileCounter>();
            scoreManager = gameTimer.GetComponent<ScoreManager>();
        }
        shakeLongWaiter = new WaitForSeconds(tileP.shakeActivationTime);
        shakeWaiter = new WaitForSeconds(tileP.shakeFrequency);
        if (this.enabled)
        {
            RegenGrid();
        }
        tileMov = GetComponent<TileMovements>();
        GridUtils.onLevelMapLoad += OnLoadScene;
    }

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLoadScene;
        editorFlag = true;
    }

    private void OnLoadScene()
    {
        if(gameTimer != null) Destroy(gameTimer.gameObject);
        if (!isHub)
        {
            previousCenterTile.transform.GetChild(9).gameObject.SetActive(false);
            centerTile.transform.GetChild(9).gameObject.SetActive(true);

            foreach (GameTimer gt in RessourcesManager.Instance.gameManagers)
            {
                if (gt.gameObject.name.Split('_')[0] == fileName)
                {
                    gameTimer = Instantiate(gt);
                    break;
                }
            }
            compassManager = gameTimer.GetComponent<CompassMissionManager>();
            tileCounter = gameTimer.GetComponent<TileCounter>();
            scoreManager = gameTimer.GetComponent<ScoreManager>();
        }

        Item[] items = FindObjectsOfType<Item>();
        for (int i = 0; i < items.Length; i++)
        {
            ObjectPooling.SharedInstance.RemovePoolItem(0, items[i].gameObject, items[i].GetType().ToString());

            //Destroy(items[i].gameObject);
        }
    }

    private void Update()
    {
        if (!isHub)
        {
            timerInterpolateValue += Time.deltaTime * (1 / gameTimer.gameTimer);
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
        if (tiles == null || tiles.Length == 0) return;
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            tile.degradationTimerMin = tileP.degradationTileTimerMin;
            tile.degradationTimerMax = tileP.degradationTileTimerMax;
            tile.degradationTimerAnimCurve = tileP.degradationTimerAnimCurve;
            tile.heightByTile = tileP.heightByTile;
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
        if (TileSystem.Instance == null) TileSystem.Instance = tileS.transform.GetComponent<TileSystem>();
        if (tileS.tiles == null) return;
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
            if (TileSystem.Instance == null) TileSystem.Instance = tileS.transform.GetComponent<TileSystem>();
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
            GameTimer gameManager = null;
            if (!TileSystem.Instance.isHub) gameManager = FindObjectOfType<GameTimer>();
            GridUtils.GenerateMap(GridUtils.GenerateMapContent(), gameManager);
            //GenerateMapContent(tileS.tiles);
            //GenerateMap(_content);
        }

        if (tileS.InstantiateGrid)
        {
            InstantiateGrid();
            UpdateGridParameters();
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
                _content += t[x, y].name;
                _content += "+";
                //1
                _content += "Walkable :";
                if (t[x, y].walkable) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";

                //2
                _content += "Degradable :";
                if (t[x, y].degradable) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";

                //3
                _content += "Tourbillon :";
                if (t[x, y].tourbillon) _content += 1.ToString();
                else _content += 0.ToString();
                _content += "+";

                //4
                _content += "TileType :";
                _content += Convert.ToInt32(t[x, y].tileType);
                _content += "+";

                //5
                _content += "TileSpawnType :";
                _content += Convert.ToInt32(t[x, y].tileSpawnType);
                _content += "+";

                //6
                _content += "SpawnPosition :";
                _content += Convert.ToString((int)t[x, y].spawnPositions);
                _content += "+";


                //7
                _content += "LevelName :";
                _content += t[x, y].levelName;
                if (t[x, y].levelName.Length == 0) _content += "NIVEAU VIDE";
                _content += "+";

                //8
                _content += "Height :";
                _content += t[x, y].transform.position.y;
                _content += "+";

                //9
                _content += "ItemSpawner :";
                if (t[x, y].TryGetComponent<ItemSpawner>(out ItemSpawner itemSpawner))
                {
                    string itemType = itemSpawner.itemToSpawn.ToString();
                    _content += itemType;
                    _content += ";";
                    _content += itemSpawner.spawnTimer.ToString();
                    _content += ";";
                    _content += Convert.ToInt32(itemSpawner.loop);
                    _content += ";";
                    _content += Convert.ToInt32(itemSpawner.spawnPosition);
                    _content += ";";
                    if (itemSpawner.itemToSpawn == SpawnableItems.Chantier) _content += itemSpawner.otherRecette;
                    else if (itemSpawner.itemToSpawn == SpawnableItems.Etabli) _content += itemSpawner.recette.ToString();
                    else if (itemSpawner.itemToSpawn == SpawnableItems.Crate || itemSpawner.itemToSpawn == SpawnableItems.Mimic) _content += itemSpawner.crateReward.ToString();
                }
                else
                {
                    _content += "No Spawner";
                }

                _content += "%\n";
            }
            _content += "|";
        }
        _content += '£';
        if (tileS.centerTile.coordX < 10) _content += 0 + "" + tileS.centerTile.coordX;
        else _content += tileS.centerTile.coordX;
        if (tileS.centerTile.coordY < 10) _content += 0 + "" + tileS.centerTile.coordY;
        else _content += tileS.centerTile.coordY;

    }

    void GenerateMap(string content)
    {
        string path = Application.streamingAssetsPath + "/LevelMaps/" + "TM_" + SceneManager.GetActiveScene().name + ".txt";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, content);
        if (!tileS.isHub)
        {
            string gameManPath = Application.dataPath + "/Prefab/GameManagers/" + SceneManager.GetActiveScene().name + "_GM.prefab";
            GameObject gameManager = FindObjectOfType<GameTimer>().gameObject;
            GameTimer manag = PrefabUtility.SaveAsPrefabAssetAndConnect(gameManager, gameManPath, UnityEditor.InteractionMode.UserAction).GetComponent<GameTimer>();
            RessourcesManager rMan = GameObject.FindObjectOfType<RessourcesManager>();
            if (!rMan.gameManagers.Contains(manag))
            {
                rMan.gameManagers.Add(manag);
                string rPath = Application.dataPath + "/Prefab/System/RessourcesManager.prefab";
                RessourcesManager rManag = PrefabUtility.SaveAsPrefabAssetAndConnect(rMan.gameObject, rPath, UnityEditor.InteractionMode.UserAction).GetComponent<RessourcesManager>();

            }
        }
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