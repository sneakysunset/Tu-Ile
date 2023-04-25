using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Boussole : Item
{
    [HideInInspector] public Tile targettedTile;
    private Transform pointer;


    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
        if (targettedTile != null)
        {
            player.pointer.gameObject.SetActive(true);
            pointer = player.pointer;
        }
    }

    public override void GrabRelease()
    {
        pointer.gameObject.SetActive(true);
        base.GrabRelease();
        pointer = null;
    }

    public override void Update()
    {
        base.Update();



        if (targettedTile != null && pointer != null)
        {
            pointer.LookAt(new Vector3(targettedTile.transform.position.x, pointer.position.y, targettedTile.transform.position.z));
        }
    }
}
