using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class TileEditorWindow : EditorWindow
{

    bool foldout;
    bool filonFoldout;
    bool walkable;
    bool degradable;
    bool vortex;
    bool groupEnabled;
    TileType tileType;
    TileType tileSpawnType;
    SpawnPositions tileSpawnPositions;
    bool spawnFilons;
    bool editPos;
    string levelName;
    float width;
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        TileEditorWindow.GetWindow(typeof(TileEditorWindow));
    }

    #region Not Implemented


    


    void PaintTileProperties()
    {
        foreach (var item in Selection.gameObjects)
        {
            if (item.GetComponent<Tile>() || item.GetComponentInParent<Tile>())
            {
                groupEnabled = false;
                goto TileSettings;
            }
        }
        groupEnabled = true;

    TileSettings:
        foldout = EditorGUILayout.Foldout(foldout, "Select a Tile");
        string foldoutSpacing = "     ";
        //width = EditorGUILayout.FloatField("Width", width);
        if (foldout)
        {
            EditorGUI.BeginDisabledGroup(groupEnabled);
            walkable = EditorGUILayout.Toggle(foldoutSpacing + "Walkable", walkable);
            degradable = EditorGUILayout.Toggle(foldoutSpacing + "Degradable", degradable);
            vortex = EditorGUILayout.Toggle(foldoutSpacing + "Vortex", vortex);
            tileType = (TileType)EditorGUILayout.EnumPopup(foldoutSpacing + "Tile Type", tileType);
            int spacing = 0;
            if (tileType == TileType.LevelLoader)
            {
                spacing += 15;
                levelName = EditorGUILayout.TextField("levelNameToLoad", levelName);
            }

            filonFoldout = EditorGUILayout.Foldout(filonFoldout, foldoutSpacing + "Filons Spawning Parameters");
            if (filonFoldout)
            {
                tileSpawnType = (TileType)EditorGUILayout.EnumPopup(foldoutSpacing + foldoutSpacing + "Filon Spawn Type", tileType);
                tileSpawnPositions = (SpawnPositions)EditorGUILayout.EnumFlagsField(foldoutSpacing + foldoutSpacing + "Filon Spawn Pos", tileSpawnPositions);
                GUILayout.BeginArea(new Rect(100, 185 + spacing, 150, 30));
                spawnFilons = GUILayout.Button("Spawn Filon");  
                GUILayout.EndArea();
                editPos = EditorGUILayout.Toggle(foldoutSpacing + foldoutSpacing + "ActivatePositionGizmos", editPos);
                GUILayout.Space(30);
            }
            EditorGUI.EndDisabledGroup();

            if (groupEnabled) return;
            foreach (var item in Selection.gameObjects)
            {
                if (item.GetComponent<Tile>()) RepaintTile(item.GetComponent<Tile>());
                else if (item.GetComponentInParent<Tile>()) RepaintTile(item.GetComponentInParent<Tile>());
            }
        }
        
    }

    void RepaintTile(Tile tile)
    {
        tile.walkable = walkable;
        tile.degradable = degradable;
        tile.tourbillon = vortex;
        EditorUtility.SetDirty(tile);
    }
    #endregion


    private void OnEnable()
    {
        Selection.selectionChanged += OnActionChange;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnActionChange;
    }

    private void OnActionChange()
    {
        Repaint();
    }

    void OnGUI()
    {
        string foldoutContent = "SelectTiles";
        bool disable = true ;
        foreach (var item in Selection.gameObjects)
        {
            if (item.TryGetComponent(out Tile tile))
            {
                if (editPos) DrawPosition(tile);
                if (spawnFilons) SpawnOnTile(tile);
                disable = false;
                foldoutContent = "Tile Functions";
            }
        }

        foldout = EditorGUILayout.Foldout(!disable, foldoutContent);
        if(foldout)
        {
            int spacing = 0;
            spacing += 23;
            GUILayout.BeginArea(new Rect(30, spacing, 150, 30));
            editPos = GUILayout.Toggle(editPos, "EditPos");
            GUILayout.EndArea();        
        
            spacing += 20;
            GUILayout.BeginArea(new Rect(30, spacing, 250, 30));
            spawnFilons = GUILayout.Button("Spawn Filon");
            GUILayout.EndArea();
        }


    }

    Transform t;
    private void DrawPosition(Tile tile)
    {
        if (t == null) t = tile.transform.GetChild(0);
        int myInt = Convert.ToInt32(tile.spawnPositions);
        bool[] bools = Utils.GetSpawnPositions(myInt);
        GUIStyle gUIStyle = new GUIStyle();
        gUIStyle.fontSize = 30;
        gUIStyle.alignment = TextAnchor.UpperLeft;
        for (int i = 0; i < bools.Length; i++)
        {
            if (bools[i])
            {
                gUIStyle.normal.textColor = Color.blue;
            }
            else
            {
                gUIStyle.normal.textColor = Color.red;
            }
            Handles.Label(t.GetChild(i).position + new Vector3(-.4f, 1, 1f), (i + 1).ToString(), gUIStyle);
        }

        EditorUtility.SetDirty(tile);
    }

    private void SpawnOnTile(Tile tile)
    {
        if (tile.tileSpawnType != TileType.Neutral)
        {
            TileMats tileM = FindObjectOfType<TileMats>();

            Transform t = tile.transform.Find("SpawnPositions");
            foreach (Transform tr in t)
            {
                foreach (Transform tp in tr)
                {
                    //DestroyImmediate(tp.gameObject);
                }
            }

            int myInt = Convert.ToInt32(tile.spawnPositions);
            bool[] bools = Utils.GetSpawnPositions(myInt);
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    if (TileSystem.Instance == null) TileSystem.Instance = FindObjectOfType<TileSystem>();
                    if (TileSystem.Instance.tileM == null) TileSystem.Instance.tileM = TileSystem.Instance.GetComponent<TileMats>();
                    SpawnItem(t.GetChild(i), tile);
                }
            }
            tile.spawnSpawners = false;
        }
        else if (tile.tileSpawnType == TileType.Neutral)
        {
            tile.spawnSpawners = false;

            foreach (Transform t in tile.transform.Find("SpawnPositions"))
            {
                foreach (Transform tp in t)
                {
                    DestroyImmediate(tp.gameObject);
                }
            }
        }

        EditorUtility.SetDirty(tile);
    }

    private GameObject SpawnItem(Transform t, Tile tile)
    {
        Interactor prefab = null;
        while (t.childCount != 0) DestroyImmediate(t.GetChild(0).gameObject);
        switch (tile.tileSpawnType)
        {
            case TileType.Wood:
                prefab = TileSystem.Instance.tileM.treePrefab;
                break;
            case TileType.Rock:
                prefab = TileSystem.Instance.tileM.rockPrefab;
                break;
            case TileType.Gold:
                prefab = TileSystem.Instance.tileM.goldPrefab;
                break;
            case TileType.Diamond:
                prefab = TileSystem.Instance.tileM.diamondPrefab;
                break;
            case TileType.Adamantium:
                prefab = TileSystem.Instance.tileM.adamantiumPrefab;
                break;
        }
        Interactor obj = PrefabUtility.InstantiatePrefab(prefab, null) as Interactor;
        obj.type = tile.tileSpawnType;
        obj.transform.parent = t;
        obj.transform.position = t.position;
        //obj.transform.LookAt(new Vector3(tile.transform.position.x, obj.transform.position.y, tile.transform.position.z));
        obj.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
        return obj.gameObject;
    }
}
#endif