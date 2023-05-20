using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public bool isOn;

    public void OptionEnable()
    {
        if(!isOn)
        {
            isOn = true;
        }
    }

    public void OnSliderMusicChange(float volume)
    {

    }

    public void OnSliderSoundChange(float sound)
    {

    }

    public void ExitOptions()
    {
        isOn = false;
    }
}
