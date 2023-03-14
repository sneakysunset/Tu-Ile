using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tile : MonoBehaviour
{
    public int coordX, coordFX, coordY;
    [SerializeField] public bool walkable = true;

    [HideInInspector] public bool isSelected;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public TileBump tileB;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public  Vector3 ogPos, currentPos;
    [HideInInspector, SerializeField] public Material disabledMat;
    [HideInInspector] public float terraFormingSpeed;
    bool selecFlag;
    [HideInInspector] public float normaliseSpeed;
    [HideInInspector] public Material unselectedMat, selectedMat, fadeMat;
    Light lightAct;
    [HideInInspector] public float capDistanceNeutraliser;
    [HideInInspector] public float bumpStrength;
    [HideInInspector] public AnimationCurve bumpDistanceAnimCurve;
    private void Start()
    {
        coordFX = coordX - coordY / 2;
        lightAct = transform.GetChild(0).GetComponent<Light>();
        ogPos = transform.position;
        currentPos = ogPos;
        tileB = GetComponent<TileBump>();
        rb = GetComponent<Rigidbody>();
        myMeshR = GetComponent<MeshRenderer>();
        if (!walkable)
        {
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            myMeshR.enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnValidate()
    {
        if(!myMeshR) myMeshR = GetComponent<MeshRenderer>();
        if (!walkable)
        {
            myMeshR.sharedMaterial = disabledMat;
        }
        else
        {
            myMeshR.sharedMaterial = unselectedMat;
        }
    }

    private void Update()
    {
        if (!isSelected && !selecFlag)
        {
            selecFlag = true;
            myMeshR.material = unselectedMat;
            lightAct.enabled = false;
        }

        NormaliseRelief();

        if (walkable && gameObject.layer == 7)
        {
            StartCoroutine(ReactiveTile());
        }
    }
    public Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = transform.localScale.x * .9f;
        Vector3 pos = ogPos + new Vector3(x * transform.localScale.x * 1.8f + xOffset, 0, z * transform.localScale.x * 1.5f);
        coordX = x;
        
        coordY = z;
        return pos;
    }

    public void OnSelected()
    {
        selecFlag = false;
        isSelected = true;
        lightAct.enabled = true;
        myMeshR.material = selectedMat;
    }

    private void NormaliseRelief()
    {
        if (!isSelected && currentPos != ogPos)
        {
            float incrementValue = Mathf.Clamp(Vector3.Distance(currentPos, ogPos) / capDistanceNeutraliser, 0.1f, 1) *normaliseSpeed * Time.deltaTime * 100;
            currentPos = Vector3.MoveTowards(currentPos, ogPos, incrementValue);
        }
    }

    private IEnumerator ReactiveTile()
    {
        yield return new WaitForEndOfFrame();

        gameObject.layer = 6;
        myMeshR.material = unselectedMat;
    }
}
