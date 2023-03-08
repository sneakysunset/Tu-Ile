using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public bool isSelected;
    [HideInInspector] public MeshRenderer meshR, myMeshR;
    [HideInInspector] public TileBump tileB;
    [HideInInspector] public Rigidbody rb;
    
    public bool walkable = true;
    public float maxVelocity;

    bool selecFlag;

    private void Start()
    {
        tileB = GetComponent<TileBump>();
        rb = GetComponent<Rigidbody>();
        meshR = transform.GetChild(0).GetComponent<MeshRenderer>();
        myMeshR = GetComponent<MeshRenderer>();
        if (!walkable)
        {
            gameObject.layer = LayerMask.NameToLayer("DisabledTile");
            meshR.enabled = false;
            myMeshR.enabled = false;
        }
    }

    private void Update()
    {
        if (!isSelected && !selecFlag)
        {
            selecFlag = true;
            meshR.material.color = Color.white;
        }

    }
    public Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = transform.localScale.x * .9f;
        Vector3 pos = ogPos + new Vector3(x * transform.localScale.x * 1.8f + xOffset, 0, z * transform.localScale.x * 1.5f);
        return pos;
    }

    public void OnSelected()
    {
        selecFlag = false;
        isSelected = true;
        meshR.material.color = Color.yellow;
    }

    
}
