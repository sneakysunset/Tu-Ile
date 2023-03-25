using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Bird : Item
{
    PlayerMovement pM;
    public float gravityDivider;
    private AI_Behaviour AIB;
    private AI_Movement AIM;
    private CharacterController AIC;

    private void Start()
    {
        AIB = GetComponent<AI_Behaviour>();    
        AIM = GetComponent<AI_Movement>();
        AIC = GetComponent<CharacterController>();
    }

    public override void Update()
    {
        base.Update();
        if (isHeld) AIC.enabled = false;
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);

        pM = player.GetComponent<PlayerMovement>();
        pM.gravityMultiplier /= gravityDivider;
        AIM.enabled = false;
        AIB.ClearPath();

    }

    public override void GrabRelease(Player player)
    {
        base.GrabRelease(player);
        rb.isKinematic = true;
        pM.gravityMultiplier *= gravityDivider;
        AIB.stopRefreshing = false;
        AIM.enabled = true;
        AIC.enabled = true;
    }
}
