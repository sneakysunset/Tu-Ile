using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class OptionMenu : MonoBehaviour
{
    public bool isOn;
    FMOD.Studio.Bus bus;
    FMOD.Studio.VCA musicVCA;
    private PauseMenu mPauseMenu;
    private EndMenu mEndMenu;
    public Slider ogSlider;


    private void Start()
    {
        mPauseMenu = FindObjectOfType<PauseMenu>();
        mEndMenu = FindObjectOfType<EndMenu>();
        bus = FMODUnity.RuntimeManager.GetBus("bus:/BusGroup");
        musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
    }

    private float DecibelToLinear(float value)
    {
        float linear = Mathf.Pow(10.0f, value / 20);
        return linear;
    }

    public void OptionEnable()
    {
        if(!isOn)
        {
            isOn = true;
        }
        ogSlider.Select();
    }

    public void OnSliderMusicChange(float volume)
    {
        musicVCA.setVolume(DecibelToLinear(volume));
    }

    public void OnSliderSoundChange(float sound)
    {
        bus.setVolume(DecibelToLinear(sound));
    }

    public void ExitOptions()
    {
        isOn = false;
        mPauseMenu.optionOn = false;
        EventSystem.current.SetSelectedGameObject(null);
        if (TileSystem.Instance.ready) mPauseMenu.optionButton.Select();
        else
        {
            mEndMenu.optionButton.Select();
        }
    }
}
