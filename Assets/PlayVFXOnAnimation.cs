using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVFXOnAnimation : MonoBehaviour
{
    public ParticleSystem spellUp;
    public void OnSpellUp()
    {
        spellUp.Play();
    }
}
