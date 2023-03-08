using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class TileSystem : MonoBehaviour
{
    public bool InstantiateGrid;
    public bool DestroyGrid;
    public int columns, rows;
    [HideInInspector, SerializeField] public Tile[,] tiles;
    public GameObject tilePrefab;
    [HideInInspector] public InputEvents inputs;
    public int ogSelectedTileX, ogSelectedTileY;
    static bool editorFlag = false;
    private void Awake()
    {
        RegenGrid();
    }

    private void Start()
    {
        inputs = FindObjectOfType<InputEvents>();
        inputs.selectedTile = tiles[ogSelectedTileX, ogSelectedTileY];
    }

    void RegenGrid()
    {
        Tile[] list = FindObjectsOfType<Tile>();
        tiles = new Tile[rows, columns];
        for (int i = 0; i < list.Length; i++)
        {
            int x = list[i].coordX;
            int y = list[i].coordY;
            tiles[x, y] = list[i];
            tiles[x, y].name = "tile " + x + " " + y;
        }

    }
    private void OnDisable()
    {
        editorFlag = true;
    }

    private void OnDrawGizmos()
    {
        if(editorFlag && !Application.isPlaying)
        {
            editorFlag = false;
            RegenGrid();
        }
        //print(tiles.Length);
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
        }
        if (tileS.DestroyGrid)
        {
            DestroyGrid();
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
                    tile.transform.position = tileS.tiles[i, j].indexToWorldPos(i, j, Vector3.zero);
                    tile.gameObject.name = i + "  " + j;
                    
                }
            }
            Debug.Log(tileS.tiles.GetLength(0) + " " + tileS.tiles.GetLength(1));
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
                        tile.transform.position = tempTiles[i, j].indexToWorldPos(i, j, Vector3.zero);
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