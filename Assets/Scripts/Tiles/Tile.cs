using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System;
using FMOD;
using Unity.VisualScripting;
using NaughtyAttributes;

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
    #endregion

    #region Materials
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideInInspector] public Material plaineMat, undegradableMat, sandMat, bounceMat, woodMat, rockMat, goldMat, diamondMat, adamantiumMat;
    [HideInInspector] public Mesh defaultMesh, woodMesh, rockMesh;
    [HideInInspector] public Color walkedOnColor, notWalkedOnColor;
    [HideInInspector] public Color penguinedColor;
    #endregion

    #region AI
    [HideInInspector] public bool isPathChecked;
    [HideNormalInspector] public int step;
    private TextMeshProUGUI AI_Text;
    [HideInInspector] public bool isPenguined;
    bool pSysIsPlaying;
    #endregion
    #endregion

    #region Call Methods

    private void Awake()
    {
        degSpeed = 1;
        Vector3 v = transform.position;
        v.y = -heightByTile * 5;
        AI_Text = GetComponentInChildren<TextMeshProUGUI>();   
        minableItems = transform.Find("SpawnPositions");
        coordFX = coordX - coordY / 2;
        currentPos = transform.position;
        if(this != TileSystem.Instance.centerTile) transform.position = v;
        else
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        pSys = transform.GetChild(3).GetComponent<ParticleSystem>();
        pSysCreation = transform.GetChild(7).GetComponent<ParticleSystem>();
        tourbillonT = transform.Find("Tourbillon");

        timer = UnityEngine.Random.Range(minTimer, maxTimer);
        GetAdjCoords();

        SetMatOnStart();
        int rotation = UnityEngine.Random.Range(0, 6);
        transform.Rotate(0, rotation * 60, 0);
    }
 

    private void Update()
    {
        if(transform.position.y == currentPos.y && spawning)
        {
            spawning = false;
            pSys.transform.position = new Vector3(pSys.transform.position.x, 0, pSys.transform.position.z) ;
            pSysCreation.Play();
        }

        if(transform.position!=currentPos && !shakeFlag && !tileS.isHub && walkable)
        {
            StartCoroutine(TileShake(.1f));
        }
        // StepText();
        isFaded = false;
        if (pSysIsPlaying && walkedOnto && degradable && tileType == TileType.Neutral)
        {
            pSys.Stop(); 
            myMeshR.material.color = walkedOnColor;
            pSysIsPlaying = false;
        }

        if(!walkable && tourbillon)
        {
            tourbillonT.Rotate(0, tourbillonSpeed * Time.deltaTime, 0);
        }

        if(isPenguined && myMeshR.material.color != penguinedColor && tileType == TileType.Neutral)
        {
            myMeshR.material.color = penguinedColor;

        }
        else if(!isPenguined && myMeshR.material.color == penguinedColor && tileType == TileType.Neutral)
        {
            myMeshR.material.color = walkedOnColor;
        }
    }
    bool shakeFlag;
    public IEnumerator TileShake(float magnitude)
    {
        shakeFlag = true;
        while (transform.position.y != currentPos.y)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(currentPos.x + x, transform.position.y, currentPos.z + y);

            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = currentPos;
        shakeFlag = false;
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
            myMeshR.material = getCorrespondingMat(tileType);
        }

        if (walkable && (!degradable || tileType == TileType.Sand || tileType == TileType.BouncyTile))
        {
            pSysIsPlaying = false;
            walkedOnto = true;
        }
        else if (walkable && !walkedOnto && degradable)
        {
            pSys.Play();
            pSysIsPlaying = true;
            myMeshR.material.color = notWalkedOnColor;
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

    public Material getCorrespondingMat(TileType tType)
    {
        Material mat;

        if (!walkable)
        {
            mat = disabledMat;
        }
        else if (!degradable)
        {
            mat = undegradableMat;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mat = plaineMat; break;
                case TileType.Wood: mat = woodMat; break;
                case TileType.Rock: mat = rockMat; break;
                case TileType.Gold: mat = goldMat; break;
                case TileType.Diamond: mat = diamondMat; break;
                case TileType.Adamantium: mat = adamantiumMat; break;
                case TileType.Sand: mat = sandMat; break;
                case TileType.BouncyTile: mat = bounceMat; break;
                case TileType.LevelLoader: mat = sandMat; break;
                default: mat = plaineMat; break;
            }
        }

        return mat;
    }

    public Mesh getCorrespondingMesh(TileType tType)
    {
        Mesh mesh;
        switch (tType)
        {
            case TileType.Neutral: mesh = defaultMesh; break;
            case TileType.Wood: mesh = woodMesh; break;
            case TileType.Rock: mesh = rockMesh; break;
            case TileType.Gold: mesh = rockMesh; break;
            case TileType.Diamond: mesh = rockMesh; break;
            case TileType.Adamantium: mesh = rockMesh; break;
            default: mesh = defaultMesh; break;
        }

        return mesh;
    }

    public void Spawn(float height, string stackType, float degradingSpeed)
    {
        TileType tType = (TileType)Enum.Parse(typeof(TileType), stackType);
        float rot = UnityEngine.Random.Range(0, 360);
        readyToRoll = true;
        transform.Rotate(0, rot - (rot % 60), 0);
        tileType = tType;
        spawning = true;
        walkable = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        myMeshR.enabled = true;
        myMeshF.mesh = getCorrespondingMesh(tileType);
        myMeshR.material = getCorrespondingMat(tileType);
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
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[0], ChangeRenderMode.BlendMode.Transparent);
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[1], ChangeRenderMode.BlendMode.Transparent);
            Color col = myMeshR.materials[0].color;
            Color col2 = myMeshR.materials[1].color;
            col.a = t;
            col2.a = t;
            myMeshR.materials[0].color = col;
            myMeshR.materials[1].color = col2;
        }
    }

    private void UnFadeTile()
    {
        if (!isFaded && fadeChecker)
        {
            fadeChecker = false;
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[0], ChangeRenderMode.BlendMode.Opaque);
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[1], ChangeRenderMode.BlendMode.Opaque);
            Color col = myMeshR.materials[0].color;
            Color col2 = myMeshR.materials[1].color;
            col.a = .2f;
            col2.a = .2f;
            myMeshR.material.color = col;
            myMeshR.materials[1].color = col2;
        }
    }
    #endregion

    #region Editor
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
            if (getCorrespondingMat(tileType) != null) myMeshR.sharedMaterial = getCorrespondingMat(tileType);
            if (getCorrespondingMesh(tileType) != null) myMeshF.sharedMesh = getCorrespondingMesh(tileType);
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
        Transform t = tile.transform.Find("SpawnPositions");
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
        TileMats tileM = FindObjectOfType<TileMats>();


        if (tile.spawnSpawners && tile.tileSpawnType != TileType.Neutral)
        {
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
