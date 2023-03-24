using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Stack : Item 
{
    [HideInInspector] public MeshRenderer meshR;
    [HideInInspector] public Collider col;
    public enum StackType { Wood, Rock};
    public StackType stackType;
    public int numberStacked = 0;

    private void Start()
    {
        meshR = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    public override void Update()
    {
        base.Update();
        if ((meshR.enabled || holdable) && numberStacked == 0)
        {
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
            rb.isKinematic = false;
        }
    }
}
