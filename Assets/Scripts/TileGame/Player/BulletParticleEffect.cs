using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletParticleEffect : MonoBehaviour
{
    ParticleSystem pSys;
    ParticleSystem.MainModule _psm_main;
    private void Start()
    {
        pSys = GetComponent<ParticleSystem>();
    }


    private void OnParticleCollision(GameObject other)
    {
        _psm_main = pSys.main;
        _psm_main.gravityModifierMultiplier = 3;
    }
}
