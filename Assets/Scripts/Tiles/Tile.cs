using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System;
using FMOD;
using Unity.VisualScripting;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

[System.Flags]
public enum SpawnPositions
{
    Nothing = 0,
    Pos1 = 1,
    Pos2 = 2,
    Pos3 = 4,
    Pos4 = 8,
    Pos5 = 16,
    Pos6 = 32,
    Pos7 = 64,
    Everything = 0b1111
}

public enum TileType { Neutral, Wood, Rock, Gold, Diamond, Adamantium, Sand, BouncyTile, LevelLoader, construction };


public class Tile : MonoBehaviour
{
    #region Variables
    #region MainVariables
    [Header("Type de Tile")]
    [Space(10)]
    [SerializeField] public bool walkable = true;
    private bool isMoving;
    public bool movingP { get { return isMoving; } set { if (isMoving != value) IsMoving(value); } }
    public bool degradable = true;
    [SerializeField] public TileType tileType = TileType.Neutral;

    [Space(15)]
    [Header("Spawn sur Tile")]
    [Space(10)]
    [SerializeField] public TileType tileSpawnType;
    public SpawnPositions spawnPositions;
    [SerializeField] public bool spawnSpawners;

    [HideNormalInspector] public int coordX, coordFX, coordY;
    [HideNormalInspector] public bool walkedOnto = false;
    [HideNormalInspector] public Vector3 currentPos;
    [HideInInspector] public Vector2Int[] adjTCoords;
    [HideNormalInspector] public float heightByTile;
    [HideNormalInspector] bool isFaded;
    [Space(15)]
    [Header("Other")]
    [Space(10)]
    public bool tourbillon;
    public float tourbillonSpeed;
    [HideNormalInspector] public bool sand_WalkedOnto;
     public string levelName;
    #endregion

    #region Degradation
    [HideNormalInspector] public bool isDegrading;
    [HideNormalInspector] public float timer;
    [HideNormalInspector] public float terraFormingSpeed;
    [HideNormalInspector] public float minTimer, maxTimer;
    [HideNormalInspector] public AnimationCurve degradationTimerAnimCurve;
    [HideInInspector] public float degradingSpeed;
    [HideInInspector] public float typeDegradingSpeed = 1;
    [HideNormalInspector] public bool isGrowing;
    [HideInInspector] public float degSpeed = 1;
    [HideNormalInspector] public float timeToGetToMaxDegradationSpeed;
    [HideNormalInspector] public bool sandFlag;
    #endregion

    #region Interactor Spawning
    [HideInInspector] public List<Transform> spawnPoints;
    [HideNormalInspector] public bool readyToRoll;
    bool spawning;
    #endregion

    #region Components
    [HideInInspector] public TileSystem tileS;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public MeshFilter myMeshF;
    [HideInInspector] public MeshCollider myMeshC;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform minableItems;
    [HideInInspector] public Transform tourbillonT;
    [HideInInspector] private ParticleSystem pSys;
    [HideInInspector] ParticleSystem pSysCreation;
    [HideInInspector] Tile_Degradation tileD;
    #endregion

    #region Materials
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideNormalInspector] public Material falaiseMat, plaineMatTop, plaineMatBottom, undegradableMat, sandMatTop, sandMatBottom, bounceMat, woodMat, rockMat, goldMat, diamondMat, adamantiumMat, centerTileMat;
    [HideNormalInspector] public Mesh defaultMesh, woodMesh, rockMesh, sandMesh, undegradableMesh, centerTileMesh, colliderMesh;
    [HideInInspector] public Color walkedOnColor, notWalkedOnColor;
    [HideInInspector] public Color penguinedColor;
     public Color falaiseColor;
    #endregion

    #region AI
    [HideInInspector] public bool isPathChecked;
    [HideNormalInspector] public int step;
    private TextMeshProUGUI AI_Text;
    [HideInInspector] public bool isPenguined;
    bool pSysIsPlaying;
    [HideNormalInspector] public Item_Etabli etabli;
    [HideNormalInspector] public LevelUI levelUI;
    [HideInInspector] public bool isNear;
    [HideInInspector] public bool IsNear { get { return isNear; } set { if (isNear != value) IsNearMethod(value); } }
    [HideInInspector] public bool isDetail;
    [HideInInspector] public bool IsDetail { get { return  isDetail; } set { if (isDetail != value) IsDetailMethod(value); } }
    #endregion
    #endregion

    #region Call Methods

    private void IsMoving(bool value)
    {
        isMoving = value;
        if (value) tileD.StartMoveSound();
        else tileD.EndMoveSound();
    }
    private void IsNearMethod(bool value)
    {
        if(!value) levelUI.PlayerFar();
        else levelUI.PlayerNear();

        isNear = value;
    }

    private void IsDetailMethod(bool value)
    {
        if(!value) levelUI.NoDetail();
        else levelUI.Detail();

        isDetail = value;
    }

    private void Awake()
    {
        if(TileSystem.Instance.isHub && tileType == TileType.LevelLoader)
        {
            Transform tr = transform.GetChild(8);
            tr.gameObject.SetActive(true);
            levelUI = tr.GetComponent<LevelUI>();
        }
        else if(!TileSystem.Instance.isHub && TileSystem.Instance.centerTile == this)
        {
            transform.GetChild(9).gameObject.SetActive(true);   
        }

        CameraCtr.startUp += OnStartUp;
        SceneManager.sceneLoaded += OnLoadScene;
        if (!walkable)
        {
            Vector3 v = transform.position;
            v.y = -heightByTile * 5;
            transform.position = v;
        }
        tileD = GetComponent<Tile_Degradation>();
        AI_Text = GetComponentInChildren<TextMeshProUGUI>();   
        minableItems = transform.Find("SpawnPositions");
        pSys = transform.GetChild(3).GetComponent<ParticleSystem>();
        pSysCreation = transform.GetChild(7).GetComponent<ParticleSystem>();
        tourbillonT = transform.Find("Tourbillon");

        degSpeed = 1;
        coordFX = coordX - coordY / 2;
        currentPos = transform.position;

        timer = UnityEngine.Random.Range(minTimer, maxTimer);

        GetAdjCoords();
        SetMatOnStart();

        if(this != TileSystem.Instance.centerTile && tileType != TileType.LevelLoader)
        {
            int rotation = UnityEngine.Random.Range(0, 6);
            transform.Rotate(0, rotation * 60, 0);
            transform.GetChild(0).Rotate(0, -rotation * 60, 0);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoadScene;
        CameraCtr.startUp -= OnStartUp;
    }

    private void OnStartUp()
    {
        readyToRoll = true;
    }

    private void OnLoadScene(Scene scene, LoadSceneMode lSM)
    {
        
        Vector3 v = transform.position;
        v.y = -heightByTile * 5;
        Vector2Int vector2Int = FindObjectOfType<CameraCtr>().tileLoadCoordinates;
        
        if (this != TileSystem.Instance.tiles[vector2Int.x, vector2Int.y]) transform.position = v;
        else
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }

    private void Update()
    {
        if(transform.position.y == currentPos.y && spawning && !TileSystem.Instance.isHub)
        {
            spawning = false;
            pSys.transform.position = new Vector3(pSys.transform.position.x, 0, pSys.transform.position.z) ;
            pSysCreation.Play();
        }

        // StepText();
        isFaded = false;
        if (pSysIsPlaying && walkedOnto && degradable && tileType != TileType.Sand && !TileSystem.Instance.isHub)
        {
            pSys.Stop();
            //myMeshR.material.color = walkedOnColor;
            Material[] mats = myMeshR.materials;
            mats[mats.Length - 1].color = walkedOnColor;
            myMeshR.materials = mats;
            pSysIsPlaying = false;
        }

        if(!walkable && tourbillon)
        {
            tourbillonT.Rotate(0, tourbillonSpeed * Time.deltaTime, 0);
        }

        if(isPenguined && myMeshR.material.color != penguinedColor && tileType != TileType.Sand)
        {
            myMeshR.material.color = penguinedColor;

        }
        else if(!isPenguined && myMeshR.material.color == penguinedColor && tileType != TileType.Sand && !tileS.isHub)
        {
            //myMeshR.material.color = walkedOnColor;
            Material[] mats = myMeshR.materials;
            mats[mats.Length - 1].color = walkedOnColor;
            myMeshR.materials = mats;
        }
    }


    private void LateUpdate()
    {
        UnFadeTile();
        isPenguined = false;
    }
    #endregion

    #region Tile Functions


    private void SetMatOnStart()
    {
        myMeshR = GetComponent<MeshRenderer>();
        myMeshF = GetComponent<MeshFilter>();
        myMeshC = GetComponent<MeshCollider>();

         if (!walkable)
        {
            walkedOnto = true;
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            myMeshR.enabled = false;
            //GetComponent<Collider>().enabled = false;
            transform.Find("Additional Visuals").gameObject.SetActive(false);
            minableItems.gameObject.SetActive(false);
        }
        else
        {
            myMeshF.mesh = getCorrespondingMesh(tileType);
            myMeshC.sharedMesh = colliderMesh;
            myMeshR.materials = getCorrespondingMat(tileType);
        }

        if (walkable && (!degradable || tileType == TileType.Sand || tileType == TileType.BouncyTile) && !tileS.isHub)
        {
            pSysIsPlaying = false;
            walkedOnto = true;
        }
        else if (walkable && !walkedOnto && degradable && !tileS.isHub)
        {
            pSys.Play();
            pSysIsPlaying = true;
            Material[] mats = myMeshR.materials;
            mats[mats.Length - 1].color = notWalkedOnColor;
            myMeshR.materials = mats;
            //myMeshR.material.color = notWalkedOnColor;
        }
/*        else if (!walkable && walkedOnto && degradable && tileType == TileType.Neutral)
        {
            myMeshR.material.color = walkedOnColor;
        }*/

        if (!walkable && tourbillon)
        {
            tourbillonT.gameObject.SetActive(true);
        }

    }

    public void Spawn(float height, string stackType, float degradingSpeed)
    {
        if (degradingSpeed == 0) degradable = false;
        TileType tType = (TileType)Enum.Parse(typeof(TileType), stackType);
        float rot = UnityEngine.Random.Range(0, 360);
        readyToRoll = true;
        transform.Rotate(0, rot - (rot % 60), 0);
        tileType = tType;
        spawning = true;
        walkable = true;
        movingP = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        myMeshR.enabled = true;
        myMeshF.mesh = getCorrespondingMesh(tileType);
        myMeshC.sharedMesh = colliderMesh;
        Material[] mats = getCorrespondingMat(tileType);
        myMeshR.materials = mats;
        typeDegradingSpeed = degradingSpeed;
        //myMeshR.material.color = walkedOnColor;
        transform.Find("Additional Visuals").gameObject.SetActive(true);
        minableItems.gameObject.SetActive(true);
        timer = UnityEngine.Random.Range(minTimer, maxTimer);
        isDegrading = false;
        transform.position = new Vector3(transform.position.x, -1.9f, transform.position.z) ;
        transform.tag = "Tile";
        currentPos.y = height - (height % heightByTile);
        isGrowing = true;
        tileS.tileC.Count();
    }
    private void GetAdjCoords()
    {
        adjTCoords = new Vector2Int[6];

        adjTCoords[2] = new Vector2Int(coordX + 1, coordY) ;
        adjTCoords[5] = new Vector2Int(coordX - 1, coordY) ;

        if(coordY % 2 == 1)
        {
            adjTCoords[0] = new Vector2Int(coordX, coordY + 1);
            adjTCoords[1] = new Vector2Int(coordX + 1, coordY + 1);
            adjTCoords[4] = new Vector2Int(coordX, coordY - 1);
            adjTCoords[3] = new Vector2Int(coordX + 1, coordY - 1);
        }
        else
        {
            adjTCoords[0] = new Vector2Int(coordX - 1, coordY + 1);
            adjTCoords[1] = new Vector2Int(coordX, coordY + 1);
            adjTCoords[4] = new Vector2Int(coordX - 1, coordY - 1);
            adjTCoords[3] = new Vector2Int(coordX, coordY - 1);
        }
    }

    [HideNormalInspector] public bool fadeChecker;
    Color currentColor;
    public void FadeTile(float t)
    {
        isFaded = true;
        if (!fadeChecker)
        {
            fadeChecker = true;
            for (int i = 0; i < myMeshR.materials.Length; i++)
            {
                ChangeRenderMode.ChangeRenderModer(myMeshR.materials[i], ChangeRenderMode.BlendMode.Transparent);
                Color col = myMeshR.materials[i].color;
                col.a = t;
                myMeshR.materials[i].color = col;
            }

        }
    }

    private void UnFadeTile()
    {
        if (!isFaded && fadeChecker)
        {
            fadeChecker = false;
            for (int i = 0; i < myMeshR.materials.Length; i++)
            {
                ChangeRenderMode.ChangeRenderModer(myMeshR.materials[i], ChangeRenderMode.BlendMode.Opaque);
                Color col = myMeshR.materials[i].color;
                col.a = .2f;
                myMeshR.materials[i].color = col;
            }
        }
    }
    #endregion

    #region Editor
    public Material[] getCorrespondingMat(TileType tType)
    {
        Material[] mat = new Material[2];
        mat[0] = falaiseMat;
        float f = UnityEngine.Random.Range(0f, 1f);
        mat[0].color = Color.Lerp(falaiseColor, Color.white, f);
        if (!walkable)
        {
            mat[1] = disabledMat;
            mat[0] = disabledMat;
        }
        else if (this == TileSystem.Instance.centerTile)
        {
            mat = new Material[1];
            mat[0] = centerTileMat;
        }
        else if (!degradable)
        {
            mat = new Material[1];
            mat[0] = undegradableMat;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mat[1] = plaineMatTop; mat[0] = plaineMatBottom; break;
                case TileType.Wood: mat = new Material[1]; mat[0] = woodMat; break;
                case TileType.Rock: mat[1] = mat[0]; mat[0] = rockMat; break;
                case TileType.Gold: mat[1] = goldMat; break;
                case TileType.Diamond: mat[1] = diamondMat; break;
                case TileType.Adamantium: mat[1] = adamantiumMat; break;
                case TileType.Sand: mat = new Material[1]; mat[0] = sandMatBottom; break;
                case TileType.BouncyTile: mat[1] = bounceMat; break;
                case TileType.LevelLoader: mat = new Material[1]; mat[0] = centerTileMat; break;
                default: mat[1] = plaineMatTop; mat[0] = plaineMatBottom; break;
            }
        }

        return mat;
    }

    public Mesh getCorrespondingMesh(TileType tType)
    {
        Mesh mesh;

        if (this == TileSystem.Instance.centerTile)
        {
            mesh = centerTileMesh;
        }
        else if (!degradable)
        {
            mesh = undegradableMesh;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mesh = defaultMesh; break;
                case TileType.Wood: mesh = woodMesh; break;
                case TileType.Rock: mesh = rockMesh; break;
                case TileType.Gold: mesh = rockMesh; break;
                case TileType.Diamond: mesh = rockMesh; break;
                case TileType.Adamantium: mesh = rockMesh; break;
                case TileType.Sand: mesh = sandMesh; break;
                case TileType.LevelLoader: mesh = centerTileMesh; break;
                default: mesh = defaultMesh; break;
            }
        }

        return mesh;
    }

#if UNITY_EDITOR
    void OnValidate() { UnityEditor.EditorApplication.delayCall += _OnValidate; }

    private void _OnValidate()
    {
        UpdateObject();
    }

    public void UpdateObject()
    {
        if (!Application.isPlaying)
        {
            if (this == null) return;
            if (!myMeshR) myMeshR = GetComponent<MeshRenderer>();
            if (!myMeshF) myMeshF = GetComponent<MeshFilter>();
            if (!myMeshC) myMeshC = GetComponent<MeshCollider>();
            minableItems = transform.Find("SpawnPositions");
            if (getCorrespondingMat(tileType) != null)
            {
                Material[] mats = getCorrespondingMat(tileType);
                if (!Application.isPlaying) myMeshR.sharedMaterials = mats;
                else myMeshR.materials = mats;
            }

            if (getCorrespondingMesh(tileType) != null)
            {
                myMeshF.sharedMesh = getCorrespondingMesh(tileType);
                myMeshC.sharedMesh = colliderMesh;
            } 
            //myMeshC.sharedMesh = myMeshF.sharedMesh;
            if (!walkable)
            {
                transform.Find("Additional Visuals").gameObject.SetActive(false);
                minableItems.gameObject.SetActive(false);
            }
            else
            {
                transform.Find("Additional Visuals").gameObject.SetActive(true);
                minableItems.gameObject.SetActive(true);
            }
            EditorUtility.SetDirty(this);
        }
    }
#endif

    public Mesh constructionMesh;
    private void OnDrawGizmos()
    {
        if(heightByTile != 0 && !Application.isPlaying)
        {
            float r = transform.position.y % heightByTile;
            transform.position = new Vector3(transform.position.x, transform.position.y - r, transform.position.z);
        }
        
        if(tileSpawnType == TileType.construction)
        {
            Gizmos.DrawMesh(constructionMesh, 0, transform.position + GameConstant.tileHeight * Vector3.up, Quaternion.identity);
        }
    }

    private void StepText()
    {
        AI_Text.text = step.ToString();
        if (!walkable && AI_Text.gameObject.activeInHierarchy) AI_Text.gameObject.SetActive(false);
        else if (walkable && !AI_Text.gameObject.activeInHierarchy) AI_Text.gameObject.SetActive(true);
    }
    #endregion
}

#region Editor Class
#if UNITY_EDITOR
[CustomEditor(typeof(Tile))]
[System.Serializable,CanEditMultipleObjects]
public class TileEditor : Editor
{
    public Tile tile;
    private void OnEnable()
    {
        tile = (Tile)target;
        
        tile.UpdateObject();
    }
    


    private void OnSceneGUI()
    {
        Draw();

        SpawnOnTile();

        EditorUtility.SetDirty(tile);
    }
        
    private void Draw()
    {
        Transform t = tile.transform.GetChild(0);
        int myInt = Convert.ToInt32(tile.spawnPositions);
        bool[] bools = Utils.GetSpawnPositions(myInt);
        GUIStyle gUIStyle = new GUIStyle();
        gUIStyle.fontSize = 30;
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
            Handles.Label(t.GetChild(i).position + Vector3.up * 1, (i + 1).ToString(), gUIStyle) ;
        }
    }

    private void SpawnOnTile()
    {


        if (tile.spawnSpawners && tile.tileSpawnType != TileType.Neutral)
        {
            TileMats tileM = FindObjectOfType<TileMats>();
            Transform t = tile.transform.Find("SpawnPositions");
            foreach (Transform tr in t)
            {
                foreach (Transform tp in tr)
                {
                    DestroyImmediate(tp.gameObject);
                }
            }

            int myInt = Convert.ToInt32(tile.spawnPositions);
            bool[] bools = Utils.GetSpawnPositions(myInt);
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    SpawnItem(t.GetChild(i), tileM);
                }
            }
            tile.spawnSpawners = false;
        }
        else if (tile.spawnSpawners && tile.tileSpawnType == TileType.Neutral)
        {
            TileMats tileM = FindObjectOfType<TileMats>();

            tile.spawnSpawners = false;

            foreach (Transform t in tile.transform.Find("SpawnPositions"))
            {
                foreach (Transform tp in t)
                {
                    DestroyImmediate(tp.gameObject);
                }
            }
        }
    }

    private GameObject SpawnItem(Transform t, TileMats tileM)
    {
        GameObject prefab = null;
        switch (tile.tileSpawnType)
        {
            case TileType.Wood:
                prefab = tileM.treePrefab;
                break;
            case TileType.Rock:
                prefab = tileM.rockPrefab;
                break;
            case TileType.Gold:
                prefab = tileM.goldPrefab;
                break;
            case TileType.Diamond:
                prefab = tileM.diamondPrefab;
                break;
            case TileType.Adamantium:
                prefab = tileM.adamantiumPrefab;
                break;
        }
        GameObject obj = PrefabUtility.InstantiatePrefab(prefab, null) as GameObject;
        Interactor inter = obj.GetComponent<Interactor>();
        inter.type = tile.tileSpawnType;
        obj.transform.parent = t;
        obj.transform.position = t.position;
        //obj.transform.LookAt(new Vector3(tile.transform.position.x, obj.transform.position.y, tile.transform.position.z));
        obj.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
        return obj;
    }
}
#endif
#endregion
