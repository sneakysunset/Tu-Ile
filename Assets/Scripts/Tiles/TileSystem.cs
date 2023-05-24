using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using AmplifyShaderEditor;
#endif
public class TileSystem : MonoBehaviour
{
    public bool InstantiateGrid;
    public bool DestroyGrid;
    public bool UpdateParameters;
    public int columns, rows;
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
    [HideInInspector] public bool ready;
    [HideNormalInspector] public float lerpingSpeed = 1;
    [HideInInspector] public float waitTimer = .3f;
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
        SceneManager.sceneLoaded += OnLoadScene;

        foreach(Tile tile in tiles)
        {
            tile.tileS = this;
        }
    }

    private void Start()
    {
        if(!isHub) gT = FindObjectOfType<GameTimer>();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoadScene;
        editorFlag = true;
    }

    private void OnLoadScene(Scene scene, LoadSceneMode lSM)
    {
        Vector2Int vector2Int = FindObjectOfType<CameraCtr>().tileLoadCoordinates;
        StartCoroutine(GridUtils.ElevateWorld(tiles[vector2Int.x, vector2Int.y]));
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
        int rowss = rows;
        int columnss = columns;

        tiles = new Tile[rowss, columnss];
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
            tile.degradationTimer = tileP.degradationTileTimer;
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
        if (TileSystem.Instance == null && !Application.isPlaying)
        {
            TileSystem.Instance = this;
        }
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

    private void OnEnable()
    {
        tileS = (TileSystem)target;
        tileS.UpdateGridParameters();
        foreach(var tile in tileS.tiles)
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

    void InstantiateGrid()
    {
        tileS.InstantiateGrid = false;
        Transform tileFolder = GameObject.FindGameObjectWithTag("TileFolder").transform;
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
                    tile.transform.position = GridUtils.indexToWorldPos(i, j, tileS.gridOgTile);
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
                        tile.transform.position = GridUtils.indexToWorldPos(i, j, tileS.gridOgTile);
                        tile.gameObject.name = i + "  " + j;
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
            tileS.tiles = new Tile[tileS.rows, tileS.columns];
            tileS.tiles = tempTiles;
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