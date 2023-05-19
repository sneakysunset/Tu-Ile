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
    public ParticleSystem featherPSYS;
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
        pM.jumpStrengthOnBounce /= jumpModifier;
        AIM.enabled = false;
        AIB.ClearPath();
        gameObject.layer = 13;
        featherPSYS.Play();
    }

    public override void GrabRelease(bool etablied)
    {
        base.GrabRelease(etablied);
        rb.isKinematic = true;
        pM.gravityMultiplier *= gravityDivider;
        pM.jumpStrength *= jumpModifier;
        pM.jumpStrengthOnBounce *= jumpModifier;
        AIB.stopRefreshing = false;
        AIM.enabled = true;
        AIC.enabled = true;
        gameObject.layer = 8;
        featherPSYS.Play();
    }

    private void OnDestroy()
    {
        if(isHeld)
        {
            GrabRelease(true);
        }
    }

    public override void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        isThrown = true;
        rb.useGravity = true;
        pM.gravityMultiplier *= gravityDivider;
        pM.jumpStrength *= jumpModifier;
        pM.jumpStrengthOnBounce *= jumpModifier;
        base.ThrowAction(player, throwStrength, direction);
        featherPSYS.Play();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Water"))
        {
            StartCoroutine(KillItem(hit.collider));
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

    public override IEnumerator KillItem(Collider other)
    {
        AIC.enabled = false;
        rb.mass /= 20;
        isThrown = true;
        StartCoroutine(base.KillItem(other));
        yield return null;
    }
}
