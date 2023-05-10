using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
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
        foreach(Tile tile in tiles)
        {
            tile.tileS = this;
        }
    }

    private void Start()
    {
        if(!isHub)
            gT = FindObjectOfType<GameTimer>();
        StartCoroutine(ElevateWorld());
    }

    private void Update()
    {
        if (!isHub)
        {
            timerInterpolateValue += Time.deltaTime * (1 / gT.gameTimer);
            degradationTimerModifier = tileP.degradationTimerAnimCurve.Evaluate(timerInterpolateValue); 
        } 
    }

    public Tile WorldPosToTile(Vector3 pos)
    {
        float xOffset = 0;
        int x;
        int z;


        z = Mathf.RoundToInt(pos.z / (tilePrefab.transform.localScale.z * 1.48f));
        if (z % 2 == 1) xOffset = tilePrefab.transform.localScale.x * .9f;
        x = Mathf.RoundToInt((pos.x - xOffset) / (tilePrefab.transform.localScale.x * 1.7f));
        
        return tiles[x, z];
    }

    public IEnumerator SinkWorld(string levelToLoad)
    {
        if (!isHub)
            StartCoroutine(gT.LerpTimeLine(gT.UIPos.anchoredPosition, gT.UIPos.anchoredPosition + Vector2.up * -100, gT.UIPos, gT.lerpCurveEaseIn, gT.lerpSpeed));
        Tile tile = centerTile;
        List<Tile> ts = new List<Tile>();
        ts.Add(tile);
        bool isOver = false;
        lerpingSpeed = .1f;
        int j = 0;
        ready = false;
        FindObjectOfType<CameraCtr>().DezoomCam();
        MissionManager.Instance.CloseMissions();
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
                        if (vecs.x >= 0 && vecs.x < rows && vecs.y >= 0 && vecs.y < columns /*&& tiles[vecs.x, vecs.y].walkable*/ && !ts.Contains(tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tiles[vecs.x, vecs.y]);
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
                else if(t == tile)
                {
                    tile.currentPos.y = 2; 
                }
                t.tourbillon = false;
                t.tourbillonT.gameObject.SetActive(false);
            }
            tile.degradable = false;

            j++;
            float timer = waitTimer / j;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                foreach(Tile t in ts)
                {
                    if (t.walkable && t != tile)
                    {
                        t.degSpeed *= 1 + Time.deltaTime * 3;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(levelToLoad);
        ready = false;
    }

    public IEnumerator ElevateWorld()
    {
        Tile tile = centerTile;
        if(!isHub)
            gT.UIPos.anchoredPosition += Vector2.up * -100;
        if (!isHub)
            StartCoroutine(gT.LerpTimeLine(gT.UIPos.anchoredPosition, gT.UIPos.anchoredPosition + Vector2.up * 100, gT.UIPos, gT.lerpCurveEaseOut, gT.lerpSpeed));
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
                        if (vecs.x >= 0 && vecs.x < rows && vecs.y >= 0 && vecs.y < columns /*&& tiles[vecs.x, vecs.y].walkable*/ && !ts.Contains(tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tiles[vecs.x, vecs.y]);
                            isOver = false;
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }


            foreach (Tile t in ts)
            {
                if (t.walkable /*&& t != tile*/)
                {
                    //t.degradable = true;
                    //t.currentPos.y = -t.heightByTile;
                    t.readyToRoll = true;
                }
                //t.tourbillon = false;
                //t.tourbillonT.gameObject.SetActive(false);
            }
            tile.degradable = false;
            j++;
            float timer = waitTimer / j;
            //float timer = .1f + Random.Range(-.1f, .3f);
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
        ready = true;

        

        foreach (Tile t in ts)
        {
            t.isPathChecked = false;
        }
    }

    public Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = tiles[x, z].transform.localScale.x * .85f;
        print(x +  " " + (x * transform.localScale.x * 1.7f + xOffset));
        Vector3 pos = ogPos + new Vector3(x * tiles[x, z].transform.localScale.x * 1.7f + xOffset, 0, z * tiles[x, z].transform.localScale.z * 1.48f);
        tiles[x, z].coordX = x;

        tiles[x, z].coordY = z;
        return pos;
    }

    public List<Tile> GetTilesAround(int numOfRows, Tile tile)
    {
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
                        if (vecs.x >= 0 && vecs.x < rows && vecs.y >= 0 && vecs.y < columns && tiles[vecs.x, vecs.y].walkable && !ts.Contains(tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tiles[vecs.x, vecs.y]);
                        }
                    }
                    ts[i].isPathChecked = true;
                }
            }
            rowsSeen++;
        }
        //ts.Remove(tile);
        foreach(Tile t in ts)
        {
            t.isPathChecked = false;
        }
        return ts;
    }

    public List<Tile> GetTilesBetweenRaws(int rowMin, int rowMax, Tile tile)
    {
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
                        if (vecs.x >= 0 && vecs.x < rows && vecs.y >= 0 && vecs.y < columns  && !ts.Contains(tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tiles[vecs.x, vecs.y]);
                            if(rowsSeen >= rowMin &&  rowsSeen <= rowMax)
                            {
                                ts2.Add(tiles[vecs.x, vecs.y]);
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

    public void Regener()
    {
        StartCoroutine(waiter());
    }

    private IEnumerator waiter()
    {
        yield return new WaitForSeconds(2);
        RegenGrid();
    }

    void RegenGrid()
    {
        Tile[] list = FindObjectsOfType<Tile>();
        if (list.Length == 0) return;
        int rowss = rows;
        int columnss = columns;
        /*if(list.Length > rows * columns)
        {
            print(this.gameObject.name + " " + list.Length + " " + rowss * columnss);

            rowss = 0;
            columnss = 0;
            TileSystem[] tileSs = FindObjectsOfType<TileSystem>();
            foreach(TileSystem tileS in tileSs)
            {
                rowss += tileS.rows;
                columns += tileS.columns;
            }
            print(list.Length + " " + rowss * columnss);
        }*/
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

    private void OnDisable()
    {
        editorFlag = true;
    }

    void UpdateGridParameters()
    {
        tileM = GetComponent<TileMats>();
        tileP = GetComponent<TileParameters>();
        foreach (var tile in tiles)
        {
            tile.maxTimer = tileP.maxTimer;
            tile.minTimer = tileP.minTimer;
            tile.degradationTimerAnimCurve = tileP.degradationTimerAnimCurve;
            tile.heightByTile = tileP.heightByTile;

            tile.plaineMat = tileM.plaineTileMat;
            tile.disabledMat = tileM.disabledTileMaterial;
            tile.sandMat = tileM.sandTileMat;
            tile.bounceMat = tileM.bounceTileMat;
            tile.undegradableMat = tileM.undegradableTileMat;
            tile.woodMat = tileM.woodTileMat;
            tile.rockMat = tileM.rockTileMat;
            tile.goldMat = tileM.goldTileMat;
            tile.diamondMat = tileM.diamondTileMat;
            tile.adamantiumMat = tileM.adamantiumTileMat;
            tile.defaultMesh = tileM.defaultTileMesh;
            tile.woodMesh = tileM.woodTileMesh;
            tile.rockMesh = tileM.rockTileMesh;
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
                    tile.transform.position = tileS.indexToWorldPos(i, j, tileS.gridOgTile);
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
                        tile.transform.position = tileS.indexToWorldPos(i, j, tileS.gridOgTile);
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
        tileS.tileM = tileS.GetComponent<TileMats>();
        tileS.tileP = tileS.GetComponent<TileParameters>();
        tileS.UpdateParameters = false;
        if (tileS.tiles == null) return;
        foreach(Tile tile in tileS.tiles)
        {
            tile.maxTimer = tileS.tileP.maxTimer;
            tile.minTimer = tileS.tileP.minTimer;
            tile.degradationTimerAnimCurve = tileS.tileP.degradationTimerAnimCurve;
            tile.heightByTile = tileS.tileP.heightByTile;

            tile.plaineMat = tileS.tileM.plaineTileMat;
            tile.disabledMat = tileS.tileM.disabledTileMaterial;
            tile.sandMat = tileS.tileM.sandTileMat;
            tile.undegradableMat = tileS.tileM.undegradableTileMat;
            tile.bounceMat = tileS.tileM.bounceTileMat;
            tile.woodMat = tileS.tileM.woodTileMat;
            tile.rockMat = tileS.tileM.rockTileMat;
            tile.goldMat = tileS.tileM.goldTileMat;
            tile.diamondMat = tileS.tileM.diamondTileMat;
            tile.adamantiumMat = tileS.tileM.adamantiumTileMat;
            tile.notWalkedOnColor = tileS.tileM.notWalkedOnColor;
            tile.walkedOnColor = tileS.tileM.walkedOnColor;
            tile.penguinedColor = tileS.tileM.acceleratedDegradationColor;
            tile.UpdateObject();
        }
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