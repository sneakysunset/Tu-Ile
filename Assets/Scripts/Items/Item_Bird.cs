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
    bool isThrown;
    private void Start()
    {
        AIB = GetComponent<AI_Behaviour>();    
        AIM = GetComponent<AI_Movement>();
        AIC = GetComponent<CharacterController>();
    }

    public override void Update()
    {
        base.Update();
        if (isHeld || isThrown) AIC.enabled = false;
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);

        pM = player.GetComponent<PlayerMovement>();
        pM.gravityMultiplier /= gravityDivider;
        pM.jumpStrength /= jumpModifier;
        AIM.enabled = false;
        AIB.ClearPath();
        gameObject.layer = 13;

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
        gameObject.layer = 8;
    }

    public override void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        isThrown = true;
        rb.useGravity = true;
        pM.gravityMultiplier *= gravityDivider;
        pM.jumpStrength *= jumpModifier;
        base.ThrowAction(player, throwStrength, direction);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Water"))
        {
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

    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if(isThrown)
        {
            isThrown = false;
            gameObject.layer = 8;
            rb.isKinematic = true;
            rb.useGravity = false;
            AIB.stopRefreshing = false;
            AIM.enabled = true;
            AIC.enabled = true;
        }
    }
}
