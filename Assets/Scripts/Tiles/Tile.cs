using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public bool isSelected;
    [HideInInspector] public MeshRenderer myMeshR;
    [HideInInspector] public TileBump tileB;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public  Vector3 ogPos, currentPos;
    public bool walkable = true;
    public float maxVelocity;
    [HideInInspector] public int coordX, coordY;
    bool selecFlag;
    [Range(0, 1)]public float normaliseSpeed;
    public Material unselectedMat, selectedMat;
    Light lightAct;
    private void Start()
    {
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
        if (!isSelected && transform.position != ogPos)
        {
            currentPos = Vector3.MoveTowards(currentPos, ogPos, normaliseSpeed);
        }
    }

    
}
