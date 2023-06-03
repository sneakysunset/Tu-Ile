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
using DG.Tweening;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine.Events;

//sing static UnityEngine.RuleTile.TilingRuleOutput;

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

public enum SpawnPosition
{
    Pos1 = 0,
    Pos2 = 1,
    Pos3 = 2,
    Pos4 = 3,
    Pos5 = 4,
    Pos6 = 5,
    Pos7 = 6
}

public enum TileType { Neutral = 0, Wood = 1, Rock = 2, Gold = 3, Diamond = 4, Adamantium = 5, Sand = 6, BouncyTile = 7, LevelLoader = 8, construction = 9 };

[SelectionBase]

public class Tile : MonoBehaviour
{
    #region V=ariables
    #region MainVariables
    [Header("Type de Tile")]
    [Space(10)]
    [SerializeField] public bool walkable = true;
    private bool isMoving;
    public bool IsMoving { get { return isMoving; } set { if (isMoving != value) IsMovingCallBack(value); } }
    public bool degradable = true;
    [SerializeField] public TileType tileType = TileType.Neutral;

    [Space(15)]
    [Header("Spawn sur Tile")]
    [Space(10)]
    [SerializeField] public TileType tileSpawnType;
    public SpawnPositions spawnPositions;
    [SerializeField] public bool spawnSpawners;

    [HideNormalInspector] public int coordX, coordFX, coordY;
    private bool walkedOntoIni = false;
    public bool walkedOnto { get { return walkedOntoIni; } set { if (walkedOnto != value) IsWalkedOntoMethod(value); } }
    [HideNormalInspector] public Vector3 currentPos;
    [HideInInspector] public Vector2Int[] adjTCoords;
    [HideNormalInspector] public float heightByTile;
    [HideNormalInspector] bool isFaded;
    [Space(15)]
    [Header("Other")]
    [Space(10)]
    public bool tourbillon;
    public float tourbillonSpeed;
     public string levelName;
    #endregion

    #region Degradation
    [HideNormalInspector] public bool isDegrading;
    [HideNormalInspector] public float timer;
    [HideNormalInspector] public float terraFormingSpeed;
    [HideNormalInspector] public float degradationTimerMin, degradationTimerMax;
    [HideNormalInspector] public AnimationCurve degradationTimerAnimCurve;
    [HideInInspector] public float degradingSpeed;
    [HideInInspector] public float typeDegradingSpeed = 1;
    [HideNormalInspector] public bool isGrowing;
    [HideInInspector] public float degSpeed = 1;
    [HideNormalInspector] public float timeToGetToMaxDegradationSpeed;
    [HideNormalInspector] public bool sandFlag;
    public float shakeMagnitude = .1f;
    public AnimationCurve shakeCurve;
    private float tourbillonOgY;
    #endregion

    #region Interactor Spawning
    public bool isHubCollider;
    public Collider hubCollider;
    [HideInInspector] public List<Transform> spawnPoints;
    [HideNormalInspector] public bool readyToRoll;
    bool spawning;
    #endregion

    #region Components
    [HideInInspector] public Transform visualRoot;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public MeshFilter myMeshF;
    [HideInInspector] public MeshCollider myMeshC;
    [HideInInspector] public Rigidbody rb;

    [HideInInspector] public Transform minableItems;
    [HideInInspector] public Transform tourbillonT;
    [HideInInspector] private ParticleSystem pSys;
    [HideInInspector] private MeshRenderer walkRunes;
    [HideInInspector] ParticleSystem pSysCreation;
    [HideInInspector] public Tile_Degradation tileD;
    #endregion

    #region Materials
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideNormalInspector] public Material undegradableMatBottom, falaiseMat, plaineMatTop, plaineMatBottom, undegradableMat, sandMatTop, sandMatBottom, bounceMat, woodMat, rockMat, goldMat, diamondMat, adamantiumMat, centerTileMat, centerTileMatBottom;
    [HideNormalInspector] public Mesh defaultMesh, woodMesh, rockMesh, sandMesh, undegradableMesh, centerTileMesh, colliderMesh;
    [HideInInspector] public Color walkedOnColor, notWalkedOnColor;
    [HideInInspector] public Color penguinedColor;
     public Color falaiseColor;
    public UnityEvent hubEventOnSpawn;
    #endregion

    public float gizmoOffset = 20;
    [HideInInspector] public Vector3 destination;
    [HideNormalInspector] public bool faded;
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
    [SerializeField] private float runeTargetAlpha;
    #endregion
    #endregion

    #region Call Methods

    public bool EditPos;

    private void IsMovingCallBack(bool value)
    {
        isMoving = value;
        if (value)
        {

            tileD.StartTileMovement();
        }
        else
        {
            if (spawning && !TileSystem.Instance.isHub)
            {
                spawning = false;
                pSys.transform.position = new Vector3(pSys.transform.position.x, 0, pSys.transform.position.z);
                pSysCreation.Play();
            }
            tileD.EndTileMovement();
        } 
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

    private void IsWalkedOntoMethod(bool value)
    {
        if(value == true && degradable && tileType != TileType.Sand && !TileSystem.Instance.isHub && walkable) tileD.StartDegradation();
        else if(value == false)
        {
            if(tileD.degradationCor != null)
            {
                StopCoroutine(tileD.degradationCor);
                tileD.degradationCor = null;
            }

            if(tileD.shakeCor != null)
            {
                StopCoroutine(tileD.shakeCor);
                tileD.shakeCor = null;
            }
        }
        walkedOntoIni = value;
    }

    private void Awake()
    {
        GridUtils.onLevelMapLoad += OnMapLoad;
        CameraCtr.startUp += OnStartUp;
        if (isHubCollider && TileSystem.Instance.isHub) hubCollider.enabled = true;
        Transform tr = transform.GetChild(8);
        levelUI = tr.GetComponent<LevelUI>();
        if(TileSystem.Instance.isHub && tileType == TileType.LevelLoader)
        {
            tr.gameObject.SetActive(true);
            transform.GetChild(9).gameObject.SetActive(true);
        }
        else if (!TileSystem.Instance.isHub && TileSystem.Instance.centerTile == this)
        {
            transform.GetChild(9).gameObject.SetActive(true);
        }
        


        rb = GetComponent<Rigidbody>();

        if (!walkable)
        {
            Vector3 v = transform.position;
            v.y = -heightByTile * 8f;
            transform.position = v;
        }
        tileD = GetComponent<Tile_Degradation>();
        AI_Text = GetComponentInChildren<TextMeshProUGUI>();   
        minableItems = transform.Find("SpawnPositions");
        pSys = transform.GetChild(3).GetComponent<ParticleSystem>();
        walkRunes = pSys.GetComponentInChildren<MeshRenderer>();
        pSysCreation = transform.GetChild(7).GetComponent<ParticleSystem>();
        tourbillonT = transform.Find("Tourbillon");
        tourbillonOgY = tourbillonT.localPosition.y;
        if (tourbillon)
        {
            tourbillonT.Rotate(0, UnityEngine.Random.Range(0f, 360f), 0);
            tourbillonT.Translate(0, UnityEngine.Random.Range(0f, 1f), 0);
            float targetPosY = tourbillonT.position.y;
            tourbillonT.position -= Vector3.up * 20;
            tourbillonT.DOMoveY(targetPosY, 5);
        }
        if(tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Play();

        degSpeed = 1;
        coordFX = coordX - coordY / 2;
        currentPos = transform.position;

        timer = UnityEngine.Random.Range(degradationTimerMin, degradationTimerMax);

        GetAdjCoords();
        SetMatOnStart();

        if(this != TileSystem.Instance.centerTile && tileType != TileType.LevelLoader)
        {
            int rotation = UnityEngine.Random.Range(0, 6);
            transform.Rotate(0, rotation * 60, 0);
            minableItems.Rotate(0, -rotation * 60, 0);
            transform.Find("SpawnPositions2").Rotate(0, -rotation * 60, 0);
        }

        if (tileType == TileType.BouncyTile) rb.isKinematic = false;
    }

    private void OnDisable()
    {
        CameraCtr.startUp -= OnStartUp;
        GridUtils.onLevelMapLoad -= OnMapLoad;
    }

    private void OnStartUp()
    {
        readyToRoll = true;
    }

    private void OnMapLoad()
    {
        if(tileType != TileType.LevelLoader && levelUI.gameObject.activeInHierarchy) levelUI.gameObject.SetActive(false);
        if (walkable)
        {
            if (isHubCollider)
            {
                if (TileSystem.Instance.isHub) hubCollider.enabled = true;
                else hubCollider.enabled = false;
            }
            if(TileSystem.Instance.isHub && tileType == TileType.LevelLoader) transform.GetChild(9).gameObject.SetActive(true);
            else if(this != TileSystem.Instance.centerTile && transform.GetChild(9).gameObject.activeInHierarchy) transform.GetChild(9).gameObject.SetActive(false);
            if (!myMeshR.enabled) myMeshR.enabled = true;
            walkedOnto = false;
            if ((!degradable || tileType == TileType.Sand || tileType == TileType.BouncyTile) && !TileSystem.Instance.isHub)
            {
                pSysIsPlaying = false;
                walkedOnto = true;
            }
            else if (!walkedOnto && degradable && !TileSystem.Instance.isHub)
            {
                pSys.Play();
                walkRunes.material.color = new Color(walkRunes.material.color.r, walkRunes.material.color.g, walkRunes.material.color.b, runeTargetAlpha);
                pSysIsPlaying = true;
            }
            int myInt = Convert.ToInt32(spawnPositions);
            bool[] bools = Utils.GetSpawnPositions(myInt);
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    Transform tr = minableItems.GetChild(i);
                    if(!tr.gameObject.activeInHierarchy)tr.gameObject.SetActive(true);
                    SpawnItem(tr);
                }
            }
            myMeshR.materials = getCorrespondingMat(tileType);
            myMeshF.mesh = getCorrespondingMesh(tileType);
            gameObject.layer = LayerMask.NameToLayer("Tile");
            transform.tag = "Tile";
            timer = UnityEngine.Random.Range(degradationTimerMin, degradationTimerMax);
            TileSystem.Instance.tileCounter.Count();
            if (tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Play();
            if (tileType == TileType.BouncyTile) rb.isKinematic = false;
            isGrowing = true;
            if (TileSystem.Instance.isHub && tileType == TileType.LevelLoader)
            {
                Transform tr = transform.GetChild(8);
                tr.gameObject.SetActive(true);
                levelUI = tr.GetComponent<LevelUI>();
            }
            tourbillonT.gameObject.SetActive(false);
            if (!pSysIsPlaying && !pSys.isPlaying && !TileSystem.Instance.isHub  && degradable)
            {
                pSys.Play();
                walkRunes.material.color = new Color(walkRunes.material.color.r, walkRunes.material.color.g, walkRunes.material.color.b, runeTargetAlpha);
            }
        }
        else if (tourbillon)
        {
            tourbillonT = transform.Find("Tourbillon");
            tourbillonT.gameObject.SetActive(true);
            tourbillonT.localPosition = Vector3.up * tourbillonOgY;
            tourbillonT.Rotate(0, UnityEngine.Random.Range(0f, 360f), 0);
            tourbillonT.Translate(0, UnityEngine.Random.Range(0f, 1f), 0);
            float targetPosY = tourbillonT.position.y;
            tourbillonT.position -= Vector3.up * 20;
            tourbillonT.DOMoveY(targetPosY, 5);
        }
        else tourbillonT.gameObject.SetActive(false);
        if (pSysIsPlaying && pSys.isPlaying && TileSystem.Instance.isHub)
        {
            pSys.Stop();
            walkRunes.material.DOFade(0, 1).SetEase(TileSystem.Instance.easeOut);
        }
        isDegrading = false;
        if(tileD.degradationCor != null && !TileSystem.Instance.isHub)
        {
            StopCoroutine(tileD.degradationCor);
            tileD.degradationCor = null;
        } 
        if(tileD.shakeCor != null && !TileSystem.Instance.isHub)
        {
            StopCoroutine(tileD.shakeCor);
            tileD.shakeCor = null;
        }
    }

    private void Update()
    {
        //isFaded = false;

        if (pSysIsPlaying && walkedOnto && degradable && tileType != TileType.Sand && !TileSystem.Instance.isHub)
        {
            pSys.Stop();
            walkRunes.material.DOFade(0, 1).SetEase(TileSystem.Instance.easeOut);
            //walkRunes.material.color = new Color(walkRunes.material.color.r, walkRunes.material.color.g, walkRunes.material.color.b, runeTargetAlpha);
            Material[] mats = myMeshR.materials;
            myMeshR.materials = mats;
            pSysIsPlaying = false;
        }
    }

    #endregion

    #region Tile Functions

    public void StopDegradation() => tileD.EndDegradation();

    private void SetMatOnStart()
    {
        visualRoot = transform.Find("TileVisuals");
        myMeshR = visualRoot.GetComponent<MeshRenderer>();
        myMeshF = visualRoot.GetComponent<MeshFilter>();
        myMeshC = GetComponent<MeshCollider>();

         if (!walkable)
        {
            walkedOnto = true;
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            myMeshR.enabled = false;
            //GetComponent<Collider>().enabled = false;
            //transform.Find("Additional Visuals").gameObject.SetActive(false);
            //minableItems.gameObject.SetActive(false);
        }
        else
        {
            myMeshF.mesh = getCorrespondingMesh(tileType);
            myMeshC.sharedMesh = colliderMesh;
            myMeshR.materials = getCorrespondingMat(tileType);
        }

        if (walkable && (!degradable || tileType == TileType.Sand || tileType == TileType.BouncyTile) && !TileSystem.Instance.isHub)
        {
            pSysIsPlaying = false;
            walkedOnto = true;
        }
        else if (walkable && !walkedOnto && degradable && !TileSystem.Instance.isHub)
        {
            pSys.Play();
            print(1);
            walkRunes.material.DOFade(0, 1).SetEase(TileSystem.Instance.easeOut);
            pSysIsPlaying = true;
            Material[] mats = myMeshR.materials;
            //mats[mats.Length - 1].color = notWalkedOnColor;
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
        if (TileSystem.Instance.isHub && !TileSystem.Instance.tutorial.tileSpawned)
        {
            TutorialManager tuto = TileSystem.Instance.tutorial;
            tuto.tileSpawned = true;
            if (tuto.enumer != null) tuto.StopCoroutine(tuto.enumer);
            tuto.enumer = tuto.GetTree();
            StartCoroutine(tuto.enumer);

        }
        transform.position = new Vector3(transform.position.x, -10, transform.position.z);
        
        if (degradingSpeed == 0) degradable = false;
        TileType tType = (TileType)Enum.Parse(typeof(TileType), stackType);
        float rot = UnityEngine.Random.Range(0, 360);
        readyToRoll = true;
        transform.Rotate(0, rot - (rot % 60), 0);
        tileType = tType;
        spawning = true;
        walkable = true;
        myMeshR.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        transform.tag = "Tile";
        timer = UnityEngine.Random.Range(degradationTimerMin, degradationTimerMax);
        myMeshF.mesh = getCorrespondingMesh(tileType);
        Material[] mats = getCorrespondingMat(tileType);
        if(!TileSystem.Instance.isHub) TileSystem.Instance.tileCounter.Count();
        if (tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Play();
        if (tileType == TileType.BouncyTile) rb.isKinematic = false;
        isGrowing = true;
        myMeshR.materials = mats;
        typeDegradingSpeed = degradingSpeed;
        //myMeshR.material.color = walkedOnColor;
        //transform.Find("Additional Visuals").gameObject.SetActive(true);
        //minableItems.gameObject.SetActive(true);
        isDegrading = false;
        transform.position = new Vector3(transform.position.x, -7f, transform.position.z) ;
        currentPos.y = height - (height % heightByTile);
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
        for (int i = 0; i < myMeshR.materials.Length; i++)
        {
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[i], ChangeRenderMode.BlendMode.Transparent);
            Color col = myMeshR.materials[i].color;
            col.a = t;
            myMeshR.materials[i].color = col;
        }
        faded = true;
    }

    public void UnFadeTile()
    {
        for (int i = 0; i < myMeshR.materials.Length; i++)
        {
            ChangeRenderMode.ChangeRenderModer(myMeshR.materials[i], ChangeRenderMode.BlendMode.Opaque);
            Color col = myMeshR.materials[i].color;
            col.a = .2f;
            myMeshR.materials[i].color = col;
        }
        faded = false;
    }

    public void SpawnItem(Transform t)
    {
        //Interactor prefab = null;
        int interactorPoolIndex = 0;
        switch (tileSpawnType)
        {
            case TileType.Wood:
                break;
            case TileType.Rock:
                interactorPoolIndex = 1;
                break;
            case TileType.Gold:
                interactorPoolIndex = 2;
                break;
            case TileType.Diamond:
                interactorPoolIndex = 3;
                break;
            case TileType.Adamantium:
                interactorPoolIndex = 4;
                break;
            default:
                interactorPoolIndex = 0;
                break;
        }

        //Interactor obj = Instantiate(prefab, null);
        //obj.type = tileSpawnType;
        Transform tr = t;
        Vector3 pos =  t.position;
        GameObject obj = ObjectPooling.SharedInstance.GetPoolItem(interactorPoolIndex, pos, tr);
        obj.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
    }

    public void SetToCenterTile(bool save)
    {
        TileSystem.Instance.centerTile.transform.GetChild(9).gameObject.SetActive(false);
        TileSystem.Instance.centerTile = this;
        myMeshR.materials = getCorrespondingMat(tileType);
        myMeshF.mesh = getCorrespondingMesh(tileType);
        transform.GetChild(9).gameObject.SetActive(true);

    }
    #endregion

    #region Editor
    public Material[] getCorrespondingMat(TileType tType)
    {
        Material[] mat = new Material[2];
        
        if (!walkable)
        {
            myMeshR.enabled = false;
            return null;
            //mat[1] = disabledMat;
            //mat[0] = disabledMat;
        }
        else if (TileSystem.Instance && ( this == TileSystem.Instance.centerTile || (tileType == TileType.LevelLoader && TileSystem.Instance.isHub)))
        {
            mat[1] = centerTileMat;
            mat[0] = centerTileMatBottom;
            if(!transform.GetChild(9).gameObject.activeInHierarchy) transform.GetChild(9).gameObject.SetActive(true);
        }
        else if (!degradable && tileType != TileType.LevelLoader)
        {
            mat[1] = undegradableMat;
            mat[0] = undegradableMatBottom;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mat[1] = plaineMatTop; mat[0] = plaineMatBottom; break;
                case TileType.Wood: mat = new Material[1]; mat[0] = woodMat; break;
                case TileType.Rock: mat = new Material[1]; mat[0] = rockMat; break;
                case TileType.Gold: mat[1] = goldMat; break;
                case TileType.Diamond: mat[1] = diamondMat; break;
                case TileType.Adamantium: mat[1] = adamantiumMat; break;
                case TileType.Sand: mat = new Material[1];  mat[0] = sandMatBottom; break;
                case TileType.BouncyTile: mat[1] = bounceMat; mat[0] = plaineMatBottom; break;
                case TileType.LevelLoader: mat = new Material[1]; mat[0] = centerTileMat; break;
                default: mat[1] = plaineMatTop; mat[0] = plaineMatBottom; break;
            }
        }
        if(walkable && !myMeshR.enabled)
        {
            myMeshR.enabled = true;
        }

        return mat;
    }

    public Mesh getCorrespondingMesh(TileType tType)
    {
        Mesh mesh;
        if (!walkable) return null;
        if (TileSystem.Instance && this == TileSystem.Instance.centerTile)
        {
            mesh = centerTileMesh;
        }
        else if (!degradable && tileType != TileType.LevelLoader)
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
            if (visualRoot == null) visualRoot = transform.Find("TileVisuals");
            if (!myMeshR) myMeshR = visualRoot.GetComponent<MeshRenderer>();
            if (!myMeshF) myMeshF = visualRoot.GetComponent<MeshFilter>();
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
                //minableItems.gameObject.SetActive(false);
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
        if(heightByTile != 0 && !Application.isPlaying && transform.position.y % heightByTile != 0)
        {
            float r = transform.position.y % heightByTile;
            transform.position = new Vector3(transform.position.x, transform.position.y - r, transform.position.z);
        }
        
        if(tileSpawnType == TileType.construction)
        {
            Gizmos.DrawMesh(constructionMesh, 0, transform.position + GameConstant.tileHeight * Vector3.up, Quaternion.identity);
        }

        if (tourbillon && !Application.isPlaying) Gizmos.DrawWireCube(transform.position + Vector3.up * 23, Vector3.one);
    }
    public void MovePoint(Vector3 pos)
    {
        destination = new Vector3(transform.position.x, pos.y, transform.position.z);
        transform.position = destination - Vector3.up * gizmoOffset;

    }
    private void StepText()
    {
        AI_Text.text = step.ToString();
        if (!walkable && AI_Text.gameObject.activeInHierarchy) AI_Text.gameObject.SetActive(false);
        else if (walkable && !AI_Text.gameObject.activeInHierarchy) AI_Text.gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireMesh(colliderMesh, 0, transform.localPosition, transform.localRotation, transform.localScale);
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
    UnityEngine.Transform t;
    Tool LastTool = Tool.None;
    private void OnEnable()
    {
        tile = (Tile)target;
        LastTool = Tools.current;
        Tools.current = Tool.None;
        tile.UpdateObject();
        tile.destination = tile.transform.position + Vector3.up * tile.gizmoOffset;
    }
    
 
 
    void OnDisable()
    {
        Tools.current = LastTool;
    }

    private void OnSceneGUI()
    {
        //base.OnInspectorGUI();

        if (tile.EditPos) Draw();

        if(tile.spawnSpawners) SpawnOnTile();

        HandleMovement();
    }

    void HandleMovement()
    {
        Handles.color = Color.red;
        Vector3 newPosA = Handles.FreeMoveHandle(tile.destination, Quaternion.LookRotation(Vector3.up, Vector3.right), 1f, Vector3.zero, Handles.ConeHandleCap);
       
        if (tile.destination != newPosA)
        {
            Undo.RecordObject(tile, "MovePoint");
            tile.MovePoint(newPosA);
        }
    }

    private void Draw()
    {
        if(t == null) t = tile.minableItems;
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
            Handles.Label(t.GetChild(i).position + new Vector3(-.4f, 1, 1f), (i + 1).ToString(), gUIStyle) ;
        }

        EditorUtility.SetDirty(tile);

        
    }





    private void SpawnOnTile()
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
                    if(TileSystem.Instance.tileM == null)TileSystem.Instance.tileM = TileSystem.Instance.GetComponent<TileMats>();
                    SpawnItem(t.GetChild(i));
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



    public GameObject SpawnItem(Transform t)
    {
        Interactor prefab = null;
        while(t.childCount != 0) DestroyImmediate(t.GetChild(0).gameObject);
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
#endregion
