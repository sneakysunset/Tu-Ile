using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public void PlayMoove()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Moove");
    }
    public void PlayMining()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Wood_Cutting");
    }
}
