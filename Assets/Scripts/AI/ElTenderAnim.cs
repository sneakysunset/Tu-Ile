using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElTenderAnim : MonoBehaviour
{
    Animator[] animators;
    AI_Movement AIM;
    Item_Bird itB;
    private void Start()
    {
        animators = GetComponentsInChildren<Animator>();
        AIM = GetComponent<AI_Movement>();
        itB = GetComponent<Item_Bird>();
    }

    private void Update()
    {
        if(itB.isHeld)
        {
            foreach(Animator animator in animators) 
            {
                animator.Play("Grabbed", 0);
            }
        }
        else if (AIM.isMoving)
        {
            foreach (Animator animator in animators)
            {
                animator.Play("Walking", 0);
            }
        }
        else
        {
            foreach (Animator animator in animators)
            {
                animator.Play("Idle", 0);
            }
        }
    }
}
