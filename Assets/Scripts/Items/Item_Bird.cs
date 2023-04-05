using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Bird : Item
{
    PlayerMovement pM;
    public float gravityDivider;
    public float jumpModifier;
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
        pM.jumpStrength /= jumpModifier;
        AIM.enabled = false;
        AIB.ClearPath();

    }

    public override void GrabRelease(Player player)
    {
        base.GrabRelease(player);
        rb.isKinematic = true;
        pM.gravityMultiplier *= gravityDivider;
        pM.jumpStrength *= jumpModifier;
        AIB.stopRefreshing = false;
        AIM.enabled = true;
        AIC.enabled = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Water"))
        {
            print(1);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Water_fall");
            if (_player)
            {
                _player.heldItem = null;
                if (_player.holdableItems.Contains(this))
                {
                    _player.holdableItems.Remove(this);
                }
            }
            Destroy(gameObject);
        }
    }
}
