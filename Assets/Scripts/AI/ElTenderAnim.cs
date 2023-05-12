using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElTenderAnim : MonoBehaviour
{
    
    Animator[] animators;
    AI_Movement AIM;
    Item_Bird itB;
    bool isHeldChecker;
    private FMOD.Studio.EventInstance fmodGrabbed;
    private FMOD.Studio.EventInstance fmodIdle;

    private void Start()
    {
        fmodGrabbed = FMODUnity.RuntimeManager.CreateInstance("event:/Tuile/Monster/Angry_Chicken");
        fmodIdle = FMODUnity.RuntimeManager.CreateInstance("event:/Tuile/Monster/Idle_Chicken");
        fmodIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        fmodGrabbed.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        fmodIdle.start();
        animators = GetComponentsInChildren<Animator>();
        AIM = GetComponent<AI_Movement>();
        itB = GetComponent<Item_Bird>();
    }

    private void Update()
    {
        fmodIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        fmodGrabbed.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        if (isHeldChecker && !itB.isHeld)
        {
            fmodIdle.start();
            if (FMODUtils.IsPlaying(fmodGrabbed))
            {
                fmodGrabbed.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        if(itB.isHeld)
        {
            if (!isHeldChecker)
            {
                fmodGrabbed.start();
                if (FMODUtils.IsPlaying(fmodIdle))
                {
                    fmodIdle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }
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
        isHeldChecker = itB.isHeld;

        
    }
    private void OnDestroy()
    {
        if (FMODUtils.IsPlaying(fmodIdle))
        {
            fmodIdle.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        if (FMODUtils.IsPlaying(fmodGrabbed))
        {
            fmodGrabbed.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        fmodGrabbed.release();
        fmodIdle.release();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Monster/Dying_Chicken");
    }
}
