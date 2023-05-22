using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Stack : Item 
{
    [HideInInspector] public MeshRenderer meshR;
    public Item_Stack.StackType stackType;


    public int numberStacked = 0;
    protected RessourcesManager rMan;
    protected MeshFilter meshF;
    protected MeshFilter meshFH;
    ressourceMeshsCollec rMC;
    public bool isTile;
    int prevNum;
    public bool trueHoldable = true;
    private void Start()
    {
        holdable = true;
        meshR = GetComponent<MeshRenderer>();
        meshF = GetComponent<MeshFilter>();
        if(meshR == null)
        {
            meshR = GetComponentInChildren<MeshRenderer>();
            meshF = GetComponentInChildren<MeshFilter>();
        }
        col = GetComponent<Collider>();
        rMan = FindObjectOfType<RessourcesManager>();
        meshFH = Highlight.GetComponent<MeshFilter>();
    }

    public override void Update()
    {
        base.Update();
        if ((meshR.enabled || holdable) && numberStacked == 0)
        {
            if(isHeld)
            {
                Destroy(this.gameObject);
            }
            meshR.enabled = false;
            col.enabled = false;
            holdable = false;
            
            rb.isKinematic = true;
        }
        else if ((!meshR.enabled || !holdable) && numberStacked >= 1)
        {
            meshR.enabled = true;
            col.enabled = true;
            holdable = true;
            if(physic)
            rb.isKinematic = false;
        }

        if (prevNum != numberStacked && !isTile) ChangeMesh();
        prevNum = numberStacked;

    }

    public override void GrabRelease(bool etablied)
    {
        base.GrabRelease(etablied);
    }

    public void ChangeMesh()
    {
        foreach (ressourceMeshsCollec r in rMan.RessourceMeshs)
        {
            if (r.stackType == stackType)
            {
                rMC = r;
                break;
            }
        }

        for (int i = 0; i < rMC.necessaryNum.Count; i++)
        {
            if (rMC.necessaryNum[i] >= numberStacked)
            {
                if (meshF.mesh != rMC.meshs[i])
                {
                    meshF.mesh = rMC.meshs[i];
                    meshFH.mesh = rMC.meshs[i];
                    meshR.material = rMC.materials[i];
                }
                return;
            }
        }
        if (meshF.mesh != rMC.meshs[rMC.necessaryNum.Count - 1])
        {
            meshF.mesh = rMC.meshs[rMC.necessaryNum.Count - 1];
            meshFH.mesh = rMC.meshs[rMC.necessaryNum.Count - 1];
            meshR.material = rMC.materials[rMC.necessaryNum.Count - 1];
        }
    }
}
