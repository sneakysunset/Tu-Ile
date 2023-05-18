using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODUtils 
{
    static public bool IsPlaying(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    static public void SetFMODEvent(ref FMOD.Studio.EventInstance instance, string eventName, Transform transform)
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(eventName);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        instance.start();
    }

    static public void StopFMODEvent(ref FMOD.Studio.EventInstance instance, bool allowFadeOut)
    {
        if (allowFadeOut) instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        else instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
