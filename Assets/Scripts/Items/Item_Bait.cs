using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Bait : Item
{
    public override void Awake()
    {
        base.Awake();
        holdable = true;
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
    }

    public override void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        base.ThrowAction(player, throwStrength, direction);
    }

    public override void GrabRelease(bool etablied)
    {
        base.GrabRelease(etablied);
    }
}
