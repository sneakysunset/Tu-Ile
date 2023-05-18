using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomnizeState : StateMachineBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float t = Random.Range(0f, 1f);

        animator.SetFloat("IdleRand", t);

        animator.SetFloat("DanceRand", t);

        animator.SetFloat("JumpRand", t);

    }


}
