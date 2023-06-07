using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Stack_Tile : Item_Stack
{
    public float degradingSpeed = 1;

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
        player.tileSelec.tileBluePrint.GetComponent<MeshFilter>().mesh = RessourcesManager.Instance.GetMeshByStackType(stackType); 
    }
}
