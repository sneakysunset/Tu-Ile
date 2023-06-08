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

[System.Serializable]
public struct tileTypeList
{
    public TileType type;
    public float tileMult;
    public int scoreValueOnElevate;
    public int scoreValueOnCreate;
}

[SelectionBase]
public class Tile : MonoBehaviour
{
    #region Variables
    #region Serialized Variables
    [Header("Tile Type")]
    [Space(10)]
    public bool walkable = true;
    public bool degradable = true;
    public TileType tileType = TileType.Neutral;
    public bool tourbillon;

    [Space(15)]
    [Header("Spawn sur Tile")]
    [Space(10)]
    public TileType tileSpawnType;
    public SpawnPositions spawnPositions;
    public bool spawnSpawners;
    public bool EditPos;
    [Space(15)]
    [Header("Other")]
    [Space(10)]
    public string levelName;
    public bool isHubCollider;
    [SerializeField] private float runeTargetAlpha;
    [SerializeField] private float runeFadeOutDuration;
    public TileComponentReferencer tc;
    public Tile_Degradation td;
    public float gizmoOffset = 20;
    [SerializeField] private tileTypeList[] tileTypeValues;
    #endregion

    #region MainVariables
    private bool isMoving;
    public bool IsMoving { get { return isMoving; } set { if (isMoving != value) IsMovingCallBack(value); } }

    [HideNormalInspector] public int coordX, coordY;
    private bool walkedOntoIni = false;
    public bool walkedOnto { get { return walkedOntoIni; } set { if (walkedOnto != value) IsWalkedOntoMethod(value); } }
    public Vector3 currentPos { get;  private set; }
    [HideInInspector] public Vector2Int[] adjTCoords;
    #endregion

    #region Degradation
    [HideNormalInspector] public bool isDegrading;
    [HideNormalInspector] public float timer;
    [HideNormalInspector] public float terraFormingSpeed;

    [HideInInspector] public float degradingSpeed;
    [HideInInspector] public float typeDegradingSpeed = 1;
    [HideNormalInspector] public bool isGrowing;
    [HideInInspector] public float degSpeed = 1;
    [HideNormalInspector] public bool sandFlag;
    private float tourbillonOgY;
    #endregion

    #region Interactor Spawning

    [HideInInspector] public List<Transform> spawnPoints;
    [HideNormalInspector] public bool readyToRoll;
    bool spawning;
    #endregion
    [HideInInspector] public Vector3 destination;
    [HideNormalInspector] public bool faded;
    #region AI
    [HideInInspector] public bool isPathChecked;
    [HideNormalInspector] public int step;
    [HideNormalInspector] public Item_Etabli etabli;
    [HideInInspector] public bool isNear;
    [HideInInspector] public bool IsNear { get { return isNear; } set { if (isNear != value) IsNearMethod(value); } }
    [HideInInspector] public bool isDetail;
    [HideInInspector] public bool IsDetail { get { return  isDetail; } set { if (isDetail != value) IsDetailMethod(value); } }
    private string groundType;
    #endregion
    #endregion

    #region Call Methods


    private void IsMovingCallBack(bool value)
    {
        isMoving = value;
        if (value)
        {

            td.StartTileMovement();
        }
        else
        {
            if (spawning && !TileSystem.Instance.isHub)
            {
                spawning = false;
                tc.undegradableParticleSystem.transform.position = new Vector3(tc.undegradableParticleSystem.transform.position.x, 0, tc.undegradableParticleSystem.transform.position.z);
                tc.pSysCreation.Play();
            }
            tc.tileD.EndTileMovement();
        } 
    }

    private void IsNearMethod(bool value)
    {
        if(!value) tc.levelUI.PlayerFar();
        else tc.levelUI.PlayerNear();

        isNear = value;
    }

    private void IsDetailMethod(bool value)
    {
        if(!value) tc.levelUI.NoDetail();
        else tc.levelUI.Detail();

        isDetail = value;
    }

    private void IsWalkedOntoMethod(bool value)
    {
        if(value == true && degradable && tileType != TileType.Sand && !TileSystem.Instance.isHub && walkable)
        {
            td.StartDegradation();
            ActivateWalkedOntoPsys(false, true);
        }
        else if(value == false && !TileSystem.Instance.isHub)
        {
            td.EndDegradation();
        }
        walkedOntoIni = value;
    }

    private void Awake()
    {
        GridUtils.onEndLevel += OnEndLevel;
        GridUtils.onLevelMapLoad += OnMapLoad;
        GridUtils.onStartLevel += OnStartLevel;

        if (tileType == TileType.LevelLoader) tc.levelUI.enabled = true;
        if (isHubCollider && TileSystem.Instance.isHub) tc.hubCollider.enabled = true;
        readyToRoll = true;


        if (TileSystem.Instance.isHub && tileType == TileType.LevelLoader)
        {
            tc.levelUI.EnableUI();
            tc.pSysCenterTile.gameObject.SetActive(true);
        }
        else if (!TileSystem.Instance.isHub && TileSystem.Instance.centerTile == this)
        {
            tc.pSysCenterTile.gameObject.SetActive(true);
        }
        


        

        if (!walkable)
        {
            Vector3 v = transform.position;
            v.y = -td.heightByTile * 8f;
            transform.position = v;
        }
        
        tourbillonOgY = tc.tourbillonT.localPosition.y;
        if (tourbillon)
        {
            tc.tourbillonT.Rotate(0, UnityEngine.Random.Range(0f, 360f), 0);
            tc.tourbillonT.Translate(0, UnityEngine.Random.Range(0f, 1f), 0);
            float targetPosY = tc.tourbillonT.position.y;
            tc.tourbillonT.position -= Vector3.up * 20;
            tc.tourbillonT.DOMoveY(targetPosY, 5);
        }
        if(tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Play();

        degSpeed = 1;
        currentPos = transform.position;

        timer = UnityEngine.Random.Range(td.degradationTimerMin, td.degradationTimerMax);

        GetAdjCoords();
        SetMatOnStart();

        if(this != TileSystem.Instance.centerTile && tileType != TileType.LevelLoader)
        {
            int rotation = UnityEngine.Random.Range(0, 6);
            transform.Rotate(0, rotation * 60, 0);
            tc.minableItems.Rotate(0, -rotation * 60, 0);
            tc.altSpawnPositions.Rotate(0, -rotation * 60, 0);
        }
    }

    private void OnDisable()
    {
        GridUtils.onEndLevel -= OnEndLevel;
        GridUtils.onLevelMapLoad -= OnMapLoad;
        GridUtils.onStartLevel -= OnStartLevel;
    }

    private void OnEndLevel(Tile tile)
    {
        /*        if (td.degradationCor != null && !TileSystem.Instance.isHub)
                {
                    StopCoroutine(td.degradationCor);
                    td.degradationCor = null;
                }
                if (td.shakeCor != null && !TileSystem.Instance.isHub)
                {
                    StopCoroutine(td.shakeCor);
                    td.shakeCor = null;
                }*/
        td.EndDegradation();
    }

    private void OnMapLoad(string p)
    {
        tc.pSysCenterTile.gameObject.SetActive(false);
        tc.tourbillonT.gameObject.SetActive(false);
        ActivateWalkedOntoPsys(false, false);
        walkedOntoIni = false;
        if(TileSystem.Instance.isHub)OnHubLoad();
        else OnLevelLoad();
        if (walkable)
        {
            tc.myMeshR.materials = getCorrespondingMat(tileType);
            tc.myMeshF.mesh = getCorrespondingMesh(tileType);
            tc.myMeshR.enabled = true;
            FilonLoader();
            
            gameObject.layer = LayerMask.NameToLayer("Tile");
            transform.tag = "Tile";
            if (tileType == TileType.Sand) tc.sandParticleSystem.Play();
            isGrowing = true;
            tc.tourbillonT.gameObject.SetActive(false);
        }
        else if (tourbillon) ActivateVortex();
    }

    private void OnStartLevel()
    {
        if (this == TileSystem.Instance.centerTile) tc.levelUI.EnableUI();
    }

    private void OnHubLoad()
    {
        if (isHubCollider) tc.hubCollider.enabled = true;
        if(tileType == TileType.LevelLoader) tc.levelUI.EnableUI();
        if (TileSystem.Instance.centerTile == this || tileType == TileType.LevelLoader) tc.pSysCenterTile.gameObject.SetActive(true);
    }

    private void OnLevelLoad()
    {
        if (isHubCollider) tc.hubCollider.enabled = false;
        tc.levelUI.DisableUI();
        if (TileSystem.Instance.centerTile == this) tc.pSysCenterTile.gameObject.SetActive(true);
        if(walkable && degradable && tileType != TileType.Sand)
        {
            ActivateWalkedOntoPsys(true, false);
            walkedOntoIni = false;
        }
        if (degradable && walkable) typeDegradingSpeed = tileTypeValues[(int)tileType].tileMult;
    }

    private void FilonLoader() 
    {
        int myInt = Convert.ToInt32(spawnPositions);
        bool[] bools = Utils.GetSpawnPositions(myInt);
        for (int i = 0; i < bools.Length; i++)
        {
            if (bools[i])
            {
                Transform tr = tc.minableItems.GetChild(i);
                if (!tr.gameObject.activeInHierarchy) tr.gameObject.SetActive(true);
                SpawnItem(tr);
            }
        }
    }

    public int GetScoreValue(){ return tileTypeValues[(int)tileType].scoreValueOnElevate; }

    private void ActivateWalkedOntoPsys(bool activate, bool allowFadeOut)
    {
        if (activate)
        {
            tc.undegradableParticleSystem.Play();
            tc.walkRunes.material.color = new Color(tc.walkRunes.material.color.r, tc.walkRunes.material.color.g, tc.walkRunes.material.color.b, runeTargetAlpha);
            tc.walkRunes.enabled = true;
        }
        else
        {
            tc.undegradableParticleSystem.Stop();
            if (allowFadeOut) tc.walkRunes.material.DOFade(0, runeFadeOutDuration).SetEase(TileSystem.Instance.easeOut);
            else tc.walkRunes.enabled = false;
        }
    }

    private void ActivateVortex()
    {
        tc.tourbillonT.gameObject.SetActive(true);
        tc.tourbillonT.localPosition = Vector3.up * tourbillonOgY;
        tc.tourbillonT.Rotate(0, UnityEngine.Random.Range(0f, 360f), 0);
        tc.tourbillonT.Translate(0, UnityEngine.Random.Range(0f, 1f), 0);
        float targetPosY = tc.tourbillonT.position.y;
        tc.tourbillonT.position -= Vector3.up * 20;
        tc.tourbillonT.DOMoveY(targetPosY, 5);
    }

    #endregion

    #region Tile Functions
    public string GetTileType()
    {
        string groundTypeName = "Neutral";
        switch (tileType)
        {
            case TileType.Wood: groundTypeName = tileType.ToString(); break;
            case TileType.Rock: groundTypeName = tileType.ToString(); break;
            case TileType.Sand: groundTypeName = tileType.ToString(); break;
            case TileType.Gold: groundTypeName = "Rock"; break;
            case TileType.Diamond: groundTypeName = "Rock"; break;
            default: break;
        }
        return groundTypeName;
    }

    public void ChangeCurrentPos(int steps)
    {
        currentPos += steps * td.heightByTile * Vector3.up;
        if(currentPos.y != transform.position.y) IsMoving = true;
    }

    public void SetCurrentPos(float height)
    {
        currentPos = height * Vector3.up;
        if(currentPos.y != transform.position.y) IsMoving = true;
    }

    public void StopDegradation() => td.EndDegradation();

    private void SetMatOnStart()
    {
         if (!walkable)
        {
            walkedOnto = true;
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            tc.myMeshR.enabled = false;
            //GetComponent<Collider>().enabled = false;
            //transform.Find("Additional Visuals").gameObject.SetActive(false);
            //minableItems.gameObject.SetActive(false);
        }
        else
        {
            tc.myMeshF.mesh = getCorrespondingMesh(tileType);
            tc.myMeshC.sharedMesh = tc.meshCollider;
            tc.myMeshR.materials = getCorrespondingMat(tileType);
        }

        if (walkable && (!degradable || tileType == TileType.Sand || tileType == TileType.BouncyTile) && !TileSystem.Instance.isHub)
        {
            walkedOnto = true;
        }
        else if (walkable && !walkedOnto && degradable && !TileSystem.Instance.isHub)
        {
            tc.undegradableParticleSystem.Play();
            tc.walkRunes.material.DOFade(0, 1).SetEase(TileSystem.Instance.easeOut);
            Material[] mats = tc.myMeshR.materials;
            //mats[mats.Length - 1].color = notWalkedOnColor;
            tc.myMeshR.materials = mats;
            //myMeshR.material.color = notWalkedOnColor;
        }
/*        else if (!walkable && walkedOnto && degradable && tileType == TileType.Neutral)
        {
            myMeshR.material.color = walkedOnColor;
        }*/

        if (!walkable && tourbillon)
        {
            tc.tourbillonT.gameObject.SetActive(true);
        }

    }

    public void Spawn(float height, string stackType)
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
        tc.myMeshR.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        transform.tag = "Tile";
        timer = UnityEngine.Random.Range(td.degradationTimerMin, td.degradationTimerMax);
        tc.myMeshF.mesh = getCorrespondingMesh(tileType);
        Material[] mats = getCorrespondingMat(tileType);
        if(!TileSystem.Instance.isHub) TileSystem.Instance.tileCounter.Count();
        if (tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Play();
        if (tileType == TileType.BouncyTile) tc.rb.isKinematic = false;
        isGrowing = true;
        tc.myMeshR.materials = mats;
        typeDegradingSpeed = tileTypeValues[(int)tileType].tileMult;
        if (!TileSystem.Instance.isHub)
        {
            TileSystem.Instance.scoreManager.ChangeScore(GetScoreValue());
        }

        isDegrading = false;
        transform.position = new Vector3(transform.position.x, -7f, transform.position.z) ;
        //currentPos.y = height - (height % td.heightByTile);
        SetCurrentPos(height - (height % td.heightByTile));
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
        for (int i = 0; i < tc.myMeshR.materials.Length; i++)
        {
            ChangeRenderMode.ChangeRenderModer(tc.myMeshR.materials[i], ChangeRenderMode.BlendMode.Transparent);
            Color col = tc.myMeshR.materials[i].color;
            col.a = t;
            tc.myMeshR.materials[i].color = col;
        }
        faded = true;
    }

    public void UnFadeTile()
    {
        for (int i = 0; i < tc.myMeshR.materials.Length; i++)
        {
            ChangeRenderMode.ChangeRenderModer(tc.myMeshR.materials[i], ChangeRenderMode.BlendMode.Opaque);
            Color col = tc.myMeshR.materials[i].color;
            col.a = .2f;
            tc.myMeshR.materials[i].color = col;
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
        TileSystem.Instance.centerTile.tc.pSysCenterTile.gameObject.SetActive(false);
        TileSystem.Instance.centerTile = this;
        tc.myMeshR.materials = getCorrespondingMat(tileType);
        tc.myMeshF.mesh = getCorrespondingMesh(tileType);
        tc.pSysCenterTile.gameObject.SetActive(true);

    }
    #endregion

    #region Editor
    public Material[] getCorrespondingMat(TileType tType)
    {
        Material[] mat = new Material[2];
        
        if (!walkable)
        {
            tc.myMeshR.enabled = false;
            return null;
        }
        else if (TileSystem.Instance && ( this == TileSystem.Instance.centerTile || (tileType == TileType.LevelLoader && TileSystem.Instance.isHub)))
        {
            mat[1] = tc.centerMat;
            mat[0] = tc.centerMatBottom;
            if(!transform.GetChild(9).gameObject.activeInHierarchy) tc.pSysCenterTile.gameObject.SetActive(true);
        }
        else if (!degradable && tileType != TileType.LevelLoader)
        {
            mat[1] = tc.undegradableMat;
            mat[0] = tc.undegradableMatBottom;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mat[1] = tc.plaineMatTop; mat[0] = tc.plaineMatBottom; break;
                case TileType.Wood: mat = new Material[1]; mat[0] = tc.woodMat; break;
                case TileType.Rock: mat = new Material[1]; mat[0] = tc.rockMat; break;
                case TileType.Gold: mat[1] = tc.goldMat; break;
                case TileType.Diamond: mat[1] = tc.diamondMat; break;
                case TileType.Adamantium: mat[1] = tc.adamantiumMat; break;
                case TileType.Sand: mat = new Material[1];  mat[0] = tc.desertMatBottom; break;
                case TileType.BouncyTile: mat[1] = tc.bounceMat; mat[0] = tc.plaineMatBottom; break;
                case TileType.LevelLoader: mat = new Material[1]; mat[0] = tc.centerMatBottom; break;
                default: mat[1] = tc.plaineMatTop; mat[0] = tc.plaineMatBottom; break;
            }
        }
        if(walkable && !tc.myMeshR.enabled)
        {
            tc.myMeshR.enabled = true;
        }

        return mat;
    }

    public Mesh getCorrespondingMesh(TileType tType)
    {
        Mesh mesh;
        if (!walkable) return null;
        if (TileSystem.Instance && this == TileSystem.Instance.centerTile)
        {
            mesh = tc.centerMesh;
        }
        else if (!degradable && tileType != TileType.LevelLoader)
        {
            mesh = tc.undegradableMesh;
        }
        else
        {
            switch (tType)
            {
                case TileType.Neutral: mesh = tc.defaultMesh; break;
                case TileType.Wood: mesh = tc.woodMesh; break;
                case TileType.Rock: mesh = tc.rockMesh; break;
                case TileType.Gold: mesh = tc.rockMesh; break;
                case TileType.Diamond: mesh = tc.rockMesh; break;
                case TileType.Adamantium: mesh = tc.rockMesh; break;
                case TileType.Sand: mesh = tc.sandMesh; break;
                case TileType.LevelLoader: mesh = tc.centerMesh; break;
                default: mesh = tc.defaultMesh; break;
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
            if (getCorrespondingMat(tileType) != null)
            {
                Material[] mats = getCorrespondingMat(tileType);
                if (!Application.isPlaying) tc.myMeshR.sharedMaterials = mats;
                else tc.myMeshR.materials = mats;
            }

            if (getCorrespondingMesh(tileType) != null)
            {
                tc.myMeshF.sharedMesh = getCorrespondingMesh(tileType);
                tc.myMeshC.sharedMesh = tc.meshCollider;
            } 
            if (!walkable)
            {
                transform.Find("Additional Visuals").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("Additional Visuals").gameObject.SetActive(true);
                tc.minableItems.gameObject.SetActive(true);
            }
            EditorUtility.SetDirty(this);
        }
    }
#endif

    public Mesh constructionMesh;
    private void OnDrawGizmos()
    {
        if(td.heightByTile != 0 && !Application.isPlaying && transform.position.y % td.heightByTile != 0)
        {
            float r = transform.position.y % td.heightByTile;
            transform.position = new Vector3(transform.position.x, transform.position.y - r, transform.position.z);
        }
        
/*        if(tileSpawnType == TileType.construction)
        {
            Gizmos.DrawMesh(constructionMesh, 0, transform.position + GameConstant.tileHeight * Vector3.up, Quaternion.identity);
        }*/

        if (tourbillon && !Application.isPlaying) Gizmos.DrawWireCube(transform.position + Vector3.up * 23, Vector3.one);
    }
    public void MovePoint(Vector3 pos)
    {
        destination = new Vector3(transform.position.x, pos.y, transform.position.z);
        transform.position = destination - Vector3.up * gizmoOffset;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireMesh(tc.meshCollider, 0, transform.localPosition, transform.localRotation, transform.localScale);
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
        if(t == null) t = tile.tc.minableItems;
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
                prefab = tile.tc.treePrefab;
                break;
            case TileType.Rock:
                prefab = tile.tc.rockPrefab;
                break;
            case TileType.Gold:
                prefab = tile.tc.goldPrefab;
                break;
            case TileType.Diamond:
                prefab = tile.tc.diamondPrefab;
                break;
            case TileType.Adamantium:
                prefab = tile.tc.adamantiumPrefab;
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
