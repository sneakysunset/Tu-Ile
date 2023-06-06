using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Crate_Mimic : Item_Crate
{
    bool isConsumed;
    AI_Behaviour AIB;
    AI_Targetting AIT;
    AI_Movement AIM;
    Animator anim;
    public override void Awake()
    {
        base.Awake();
        holdable = false;
        AIB = GetComponent<AI_Behaviour>();
        AIT = GetComponent<AI_Targetting>();
        AIM = GetComponent<AI_Movement>();
        anim = GetComponentInChildren<Animator>();
        AIM.onJump += OnJump;
    }

    private void OnDisable()
    {
        AIM.onJump -= OnJump;
    }

    public override void Update()
    {
        base.Update();
        Tile tileUnder = GridUtils.WorldPosToTile(this.gameObject.transform.position);
        if (tileUnder == TileSystem.Instance.centerTile && !isConsumed)
        {
            isConsumed = true;
            AIB.Disable();
            AIB.enabled = false;
            AIM.enabled = false;
            AIT.enabled = false;
            StartCoroutine(OnCenterReached(tileUnder.tc.minableItems));
        }
        if (isConsumed) AIM._characterController.enabled = false;

        if (AIM.isMoving && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) anim.Play("Move");
        else if (!AIM.isMoving && anim.GetCurrentAnimatorStateInfo(0).IsName("Move")) anim.Play("Idle");
    }

    private void OnJump()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) anim.Play("Jump");
    }
    public override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
    }
}
