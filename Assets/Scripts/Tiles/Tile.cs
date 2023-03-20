using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tile : MonoBehaviour
{
    #region publicVariables
    public enum TileType { Neutral, Tree, Rock };
    [SerializeField] public TileType tileType;
    [SerializeField] public bool walkable = true;
    #endregion region

    #region hiddenVariables
    [HideNormalInspector] public bool isDegrading;
    [HideNormalInspector] public float timer;
    [HideNormalInspector] public int coordX, coordFX, coordY;
    [HideInInspector] public List<Transform> spawnPoints;
    [SerializeField] public bool spawnSpawners;
    [HideNormalInspector] public bool isSelected;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public TileBump tileB;
    [HideInInspector] public Rigidbody rb;
    [HideNormalInspector] public  Vector3 ogPos, currentPos;
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideNormalInspector] public float terraFormingSpeed;
    bool selecFlag;
    [HideNormalInspector] public float normaliseSpeed;
    [HideInInspector] public TileSystem tileS;
    [HideInInspector] public Material unselectedMat, selectedMat, fadeMat;
    Light lightAct;
    [HideNormalInspector] public float capDistanceNeutraliser;
    [HideNormalInspector] public float bumpStrength;
    [HideNormalInspector] public AnimationCurve bumpDistanceAnimCurve;
    [HideNormalInspector] bool isFaded;
    [HideNormalInspector] public float minTimer, maxTimer;
    [HideNormalInspector] public AnimationCurve degradationTimerAnimCurve;
    [HideNormalInspector] public float timeToGetToMaxDegradationSpeed;
    [HideInInspector] public Vector2Int[] adjTCoords;
    private float degradationTimerModifier;
    [HideInInspector] public float degradingSpeed;
    [HideInInspector] public bool isGrowing;
    [HideNormalInspector] public float heightByTile;

    #endregion

    private void Start()
    {
        coordFX = coordX - coordY / 2;
        lightAct = transform.GetChild(0).GetComponent<Light>();
        ogPos = transform.position;
        currentPos = ogPos;  

        myMeshR = GetComponent<MeshRenderer>();
        if (!walkable)
        {
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            myMeshR.enabled = false;
            //GetComponent<Collider>().enabled = false;
            transform.Find("Additional Visuals").gameObject.SetActive(false);
        }
        timer = Random.Range(minTimer, maxTimer);
        GetAdjCoords();
    }

    private void OnValidate()
    {
        if(!myMeshR) myMeshR = GetComponent<MeshRenderer>();
        if (!walkable)
        {
            myMeshR.sharedMaterial = disabledMat;
            transform.Find("Additional Visuals").gameObject.SetActive(false);
        }
        else
        {
            myMeshR.sharedMaterial = unselectedMat;
            transform.Find("Additional Visuals").gameObject.SetActive(true);
        }
    }

    private void Update()
    {
      

        //NormaliseRelief();

        if (walkable && isFaded)
        {
            StartCoroutine(ReactiveTile());
        }

        if(walkable)Degrading();

    }
    bool degradingChecker;

    private void Degrading()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime * degradationTimerAnimCurve.Evaluate(degradationTimerModifier);
        }
        else if (timer <= 0)
        {
            isDegrading = true;
            gameObject.tag = "DegradingTile";
        }
        degradationTimerModifier += Time.deltaTime * (1 / timeToGetToMaxDegradationSpeed);

        if (!isSelected && !selecFlag)
        {
            selecFlag = true;
            myMeshR.material = unselectedMat;
            lightAct.enabled = false;
        }


        if (isDegrading && !degradingChecker && !isGrowing && walkable)
        {
            currentPos.y -= heightByTile;

        }

        if(transform.position == currentPos && isDegrading)
        {
            isDegrading = false;
            timer = Random.Range(minTimer, maxTimer);
        }

        if(currentPos == ogPos && CompareTag("DegradingTile"))
        {
            tag = "Tile";
        }

        degradingChecker = isDegrading;
        isGrowing = false;

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

    public Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = transform.localScale.x * .9f;
        Vector3 pos = ogPos + new Vector3(x * transform.localScale.x * 1.7f + xOffset, 0, z * transform.localScale.x * 1.48f);
        coordX = x;
        
        coordY = z;
        return pos;
    }

    private IEnumerator ReactiveTile()
    {
        yield return new WaitForEndOfFrame();

        gameObject.layer = 6;
        myMeshR.material = unselectedMat;
    }

    public void FadingTileEffect()
    {
        myMeshR.material = fadeMat;
        isFaded = true;
    }

    public void Spawn()
    {
        walkable = true;
        gameObject.layer = LayerMask.NameToLayer("Tile");
        myMeshR.enabled = true;
        myMeshR.material = unselectedMat;
        transform.Find("Additional Visuals").gameObject.SetActive(true);
        timer = Random.Range(minTimer, maxTimer);
        isDegrading = false;
        transform.tag = "Tile";
    }

    private void OnDrawGizmos()
    {
        if(heightByTile != 0 && !Application.isPlaying)
        {
            float r = transform.position.y % heightByTile;
            transform.position = new Vector3(transform.position.x, transform.position.y - r, transform.position.z);
        }
    }
}

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
        if (tile.spawnSpawners && tile.tileType != Tile.TileType.Neutral)
        {
            tile.spawnSpawners = false;
            if (tile.spawnPoints.Count > 0)
            {
                foreach (Transform t in tile.spawnPoints)
                {
                    DestroyImmediate(t.GetChild(0).gameObject);
                }
                tile.spawnPoints.Clear();
            }

            tile.spawnPoints = new List<Transform>();
            TileMats tileM = FindObjectOfType<TileMats>();
            foreach (Transform t in tile.transform.Find("SpawnPositions"))
            {
                if (t.gameObject.activeSelf)
                {
                    tile.spawnPoints.Add(t);
                    GameObject obj = SpawnItem(t, tileM);
                }
            }
        }
        else if (tile.spawnSpawners && tile.tileType == Tile.TileType.Neutral) 
        {
            tile.spawnSpawners = false;
            if (tile.spawnPoints.Count > 0)
            {
                foreach (Transform t in tile.spawnPoints)
                {
                    DestroyImmediate(t.GetChild(0).gameObject);
                }
                tile.spawnPoints.Clear();
            }
        }
    }

    private GameObject SpawnItem(Transform t, TileMats tileM)
    {
        GameObject prefab = null;
        switch (tile.tileType)
        {
            case Tile.TileType.Tree:
                prefab = tileM.treePrefab;
                break;
            case Tile.TileType.Rock:
                prefab = tileM.rockPrefab;
                break;
        }
        GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        obj.transform.parent = t;
        obj.transform.position = t.position;
        obj.transform.LookAt(new Vector3(tile.transform.position.x, obj.transform.position.y, tile.transform.position.z));
        
        return obj;
    }
}
#endif
