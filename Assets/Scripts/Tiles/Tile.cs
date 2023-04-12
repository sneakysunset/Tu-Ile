using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System;
using FMOD;
using Unity.VisualScripting;

public class Tile : MonoBehaviour
{
    #region Variables
    #region MainVariables
    [SerializeField] public bool walkable = true;
    [SerializeField] public TileType tileSpawnType;
    [SerializeField] public TileType tileType = TileType.Neutral;


    public enum TileType { Neutral, Wood, Rock, Gold, Diamond, Adamantium, Sand, LevelLoader };
    public string levelName;
    [HideNormalInspector] public int coordX, coordFX, coordY;
    public bool walkedOnto = false;
    [HideNormalInspector] public Vector3 currentPos;
    public float maxPos ;
    [HideInInspector] public Vector2Int[] adjTCoords;
    [HideNormalInspector] public float heightByTile;
    [HideNormalInspector] bool isFaded;
    public bool tourbillon;
    public float tourbillonSpeed;
    [HideNormalInspector] public bool sand_WalkedOnto;
    #endregion

    #region Degradation
    public bool degradable = true;
    [HideNormalInspector] public bool isDegrading;
    [HideNormalInspector] public float timer;
    [HideNormalInspector] public float terraFormingSpeed;
    [HideNormalInspector] public float minTimer, maxTimer;
    [HideNormalInspector] public AnimationCurve degradationTimerAnimCurve;
    [HideInInspector] public float degradingSpeed;
    [HideInInspector] public bool isGrowing;
    [HideNormalInspector] public float timeToGetToMaxDegradationSpeed;
    #endregion

    #region Interactor Spawning
    [SerializeField] public bool spawnSpawners;
    [HideInInspector] public List<Transform> spawnPoints;
    #endregion

    #region Components
    [HideInInspector] public TileSystem tileS;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public MeshFilter myMeshF;
    [HideInInspector] public TileBump tileB;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform minableItems;
    [HideInInspector] public Transform tourbillonT;
    [HideInInspector] private ParticleSystem pSys;
    public ParticleSystem pSysCreation;
    #endregion

    #region Materials
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideInInspector] public Material plaineMat, undegradableMat, sandMat;
    public Color walkedOnColor, notWalkedOnColor;
    public Color penguinedColor;
    private Material currentMat;
    #endregion

    #region Bump
    [HideNormalInspector] public float bumpStrength;
    [HideNormalInspector] public AnimationCurve bumpDistanceAnimCurve;
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
        AI_Text = GetComponentInChildren<TextMeshProUGUI>();   
        minableItems = transform.Find("SpawnPositions");
        coordFX = coordX - coordY / 2;
        currentPos = transform.position;
        pSys = gameObject.GetComponentInChildren<ParticleSystem>();

        tourbillonT = transform.Find("Tourbillon");

        timer = UnityEngine.Random.Range(minTimer, maxTimer);
        GetAdjCoords();

        SetMatOnStart();    
    }
 

    private void Update()
    {
        if(transform.position.y == currentPos.y && spawning)
        {
            spawning = false;
            pSys.transform.position = new Vector3(pSys.transform.position.x, 0, pSys.transform.position.z) ;
            pSysCreation.Play();
        }

        if(transform.position!=currentPos && !shakeFlag && !tileS.isHub)
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

        if (!degradable && walkable && tileType == TileType.Neutral)
        {
            myMeshR.material = plaineMat;
        }
        else if (!walkable)
        {
            walkedOnto = true;
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            myMeshR.enabled = false;
            //GetComponent<Collider>().enabled = false;
            transform.Find("Additional Visuals").gameObject.SetActive(false);
            minableItems.gameObject.SetActive(false);
        }
        else if (walkable && !degradable && tileType == TileType.Sand)
        {
            pSysIsPlaying = false;
            myMeshR.material = sandMat;
            walkedOnto = true;
        }
        else if (walkable && !walkedOnto && degradable)
        {
            pSys.Play();
            pSysIsPlaying = true;
            myMeshR.material.color = notWalkedOnColor;
        }
        else if (!walkable && walkedOnto && degradable && tileType == TileType.Neutral)
        {
            myMeshR.material.color = walkedOnColor;
        }
        if (!walkable && tourbillon)
        {
            tourbillonT.gameObject.SetActive(true);
        }
    }
    bool spawning;
    public void Spawn(float height, Material mat, Mesh mesh, string stackType)
    {
        spawning = true;
        walkable = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        myMeshR.enabled = true;
        myMeshF.mesh = mesh;
        myMeshR.material = mat;

        //myMeshR.material.color = walkedOnColor;
        transform.Find("Additional Visuals").gameObject.SetActive(true);
        minableItems.gameObject.SetActive(true);
        timer = UnityEngine.Random.Range(minTimer, maxTimer);
        isDegrading = false;
        transform.position = new Vector3(transform.position.x, -1.9f, transform.position.z) ;
        transform.tag = "Tile";
        currentPos.y = height - (height % heightByTile);
        isGrowing = true;
        TileType tType = (TileType)Enum.Parse(typeof(TileType), stackType);
        tileType = tType;
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
            ChangeRenderMode.ChangeRenderModer(myMeshR.material, ChangeRenderMode.BlendMode.Transparent);
            Color col = myMeshR.material.color;
            col.a = t;
            myMeshR.material.color = col;
        }
    }

    private void UnFadeTile()
    {
        if (!isFaded && fadeChecker)
        {
            fadeChecker = false;
            ChangeRenderMode.ChangeRenderModer(myMeshR.material, ChangeRenderMode.BlendMode.Opaque);
            Color col = myMeshR.material.color;
            col.a = .2f;
            myMeshR.material.color = col;
        }
    }
    #endregion

    #region Editor
    private void OnValidate()
    {
        if(!myMeshR) myMeshR = GetComponent<MeshRenderer>();
        minableItems = transform.Find("SpawnPositions");
        if (!walkable)
        {
            myMeshR.sharedMaterial = disabledMat;
            transform.Find("Additional Visuals").gameObject.SetActive(false);
            minableItems.gameObject.SetActive(false);
        }
        else if(tileType == TileType.Sand || tileType == TileType.LevelLoader)
        {
            myMeshR.sharedMaterial = sandMat;
            transform.Find("Additional Visuals").gameObject.SetActive(true);
            minableItems.gameObject.SetActive(true);
        }
        else if (!degradable)
        {
            myMeshR.sharedMaterial = undegradableMat;
            transform.Find("Additional Visuals").gameObject.SetActive(true);
            minableItems.gameObject.SetActive(true);
        }
        else
        {
            myMeshR.sharedMaterial = plaineMat;
            transform.Find("Additional Visuals").gameObject.SetActive(true);
            minableItems.gameObject.SetActive(true);
        }
    }
    private void OnDrawGizmos()
    {
        if(heightByTile != 0 && !Application.isPlaying)
        {
            float r = transform.position.y % heightByTile;
            transform.position = new Vector3(transform.position.x, transform.position.y - r, transform.position.z);
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
    }



    private void OnSceneGUI()
    {
        SpawnOnTile();

        EditorUtility.SetDirty(tile);
    }

    private void SpawnOnTile()
    {
        tile.spawnPoints = new List<Transform>();
        TileMats tileM = FindObjectOfType<TileMats>();
        foreach (Transform t in tile.transform.Find("SpawnPositions"))
        {

            if (t.gameObject.activeSelf)
            {
                tile.spawnPoints.Add(t);
            }
            else
            {
                tile.spawnPoints.Remove(t);
            }
        }

        if (tile.spawnSpawners && tile.tileSpawnType != Tile.TileType.Neutral)
        {
            foreach (Transform t in tile.transform.Find("SpawnPositions"))
            {
                foreach (Transform tp in t)
                {
                    DestroyImmediate(tp.gameObject);
                }
            }


            if (tile.spawnPoints.Count > 0)
            {
                foreach (Transform t in tile.spawnPoints)
                {
                    SpawnItem(t, tileM);
                }
                tile.spawnPoints.Clear();
            }
            tile.spawnSpawners = false;
        }
        else if (tile.spawnSpawners && tile.tileSpawnType == Tile.TileType.Neutral)
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
            case Tile.TileType.Wood:
                prefab = tileM.treePrefab;
                break;
            case Tile.TileType.Rock:
                prefab = tileM.rockPrefab;
                break;
            case Tile.TileType.Gold:
                prefab = tileM.goldPrefab;
                break;
            case Tile.TileType.Diamond:
                prefab = tileM.diamondPrefab;
                break;
            case Tile.TileType.Adamantium:
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
