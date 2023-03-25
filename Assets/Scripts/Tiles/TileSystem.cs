using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
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
    public GameObject tilePrefab;
    [HideInInspector] public InputEvents inputs;
    [HideInInspector] public TileParameters tileP;
    [HideInInspector] public TileMats tileM;
    public int ogSelectedTileX, ogSelectedTileY;
    public Vector3 gridOgTile;
    static bool editorFlag = false;
    public Vector2Int targetTileCoords;
    public int numOfRows;

    private void Awake()
    {
        if (this.enabled)
        {
            RegenGrid();
        }
    }

    private void Start()
    {
        inputs = FindObjectOfType<InputEvents>();
        //inputs.selectedTile = tiles[ogSelectedTileX, ogSelectedTileY];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            List<Tile> tiless = GetTilesAround(numOfRows, tiles[targetTileCoords.x, targetTileCoords.y]);
        }
    }

    public Tile WorldPosToTile(Vector3 pos)
    {
        float xOffset = 0;
        int x;
        int z;

        z = Mathf.RoundToInt(pos.z / (tilePrefab.transform.localScale.x * 1.48f));
        if (z % 2 == 1) xOffset = tilePrefab.transform.localScale.x * .9f;
        x = Mathf.RoundToInt((pos.x - xOffset) / (tilePrefab.transform.localScale.x * 1.7f));
        
        return tiles[x, z];
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
                if (!ts[i].isChecked)
                {
                    foreach (Vector2Int vecs in ts[i].adjTCoords)
                    {
                        if (vecs.x >= 0 && vecs.x < rows && vecs.y >= 0 && vecs.y < columns && tiles[vecs.x, vecs.y].walkable && !ts.Contains(tiles[vecs.x, vecs.y]))
                        {
                            ts.Add(tiles[vecs.x, vecs.y]);
                        }
                    }
                    ts[i].isChecked = true;
                }
            }
            rowsSeen++;
        }
        //ts.Remove(tile);
        foreach(Tile t in ts)
        {
            t.isChecked = false;
        }
        return ts;
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
        foreach (Tile tile in tiles)
        {
            tile.terraFormingSpeed = tileP.terraFormingSpeed;
            tile.normaliseSpeed = tileP.terraFormingNormalisingSpeed;
            tile.capDistanceNeutraliser = tileP.distanceSpeedNormaliserModifier;
            tile.bumpStrength = tileP.bumpStrength;
            tile.bumpDistanceAnimCurve = tileP.bumpDistanceCurve;
            tile.selectedMat = tileM.selectedTileMaterial;
            tile.unselectedMat = tileM.unselectedTileMaterial;
            tile.disabledMat = tileM.disabledTileMaterial;
            tile.fadeMat = tileM.FadedTileMaterial;
            tile.maxTimer = tileP.maxTimer;
            tile.minTimer = tileP.minTimer;
            tile.degradationTimerAnimCurve = tileP.degradationTimerAnimCurve;
            tile.timeToGetToMaxDegradationSpeed = tileP.timeToGetToMaxDegradationSpeed;
            tile.degradingSpeed = tileP.degradingSpeed;
            tile.heightByTile = tileP.heightByTile;
        }
    }

    private void OnDrawGizmos()
    {
        if(editorFlag && !Application.isPlaying)
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
        if (tileS.tiles == null) tileS.tiles = new Tile[0, 0];
        if(tileS.tiles.GetLength(0) == 0)
        {
            tileS.tiles = new Tile[tileS.rows, tileS.columns];

            for (int i = 0; i < tileS.rows; i++)
            {
                for (int j = 0; j < tileS.columns; j++)
                {
                    GameObject tile = PrefabUtility.InstantiatePrefab(tileS.tilePrefab) as GameObject;
                    tile.transform.parent = tileS.transform;
                    tileS.tiles[i, j] = tile.GetComponent<Tile>();
                    tile.transform.position = tileS.tiles[i, j].indexToWorldPos(i, j, tileS.gridOgTile);
                    tile.gameObject.name = i + "  " + j;

                }
            }
            UpdateGridParameters();
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
                        GameObject tile = PrefabUtility.InstantiatePrefab(tileS.tilePrefab) as GameObject;
                        tile.transform.parent = tileS.transform;
                        tempTiles[i, j] = tile.GetComponent<Tile>();
                        tile.transform.position = tempTiles[i, j].indexToWorldPos(i, j, tileS.gridOgTile);
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
        foreach(Tile tile in tileS.tiles)
        {
            tile.terraFormingSpeed = tileS.tileP.terraFormingSpeed;
            tile.normaliseSpeed = tileS.tileP.terraFormingNormalisingSpeed;
            tile.capDistanceNeutraliser = tileS.tileP.distanceSpeedNormaliserModifier;
            tile.bumpStrength = tileS.tileP.bumpStrength;
            tile.bumpDistanceAnimCurve = tileS.tileP.bumpDistanceCurve;
            tile.selectedMat = tileS.tileM.selectedTileMaterial;
            tile.unselectedMat = tileS.tileM.unselectedTileMaterial;
            tile.disabledMat = tileS.tileM.disabledTileMaterial;
            tile.fadeMat = tileS.tileM.FadedTileMaterial;
            tile.maxTimer = tileS.tileP.maxTimer;
            tile.minTimer = tileS.tileP.minTimer;
            tile.degradationTimerAnimCurve = tileS.tileP.degradationTimerAnimCurve;
            tile.timeToGetToMaxDegradationSpeed = tileS.tileP.timeToGetToMaxDegradationSpeed;
            tile.degradingSpeed = tileS.tileP.degradingSpeed;
            tile.heightByTile = tileS.tileP.heightByTile;
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