using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    public int healthPoints;
    public ParticleSystem deathPSys;
    public void OnHit(int damages)
    {
        healthPoints -= damages;

        if(healthPoints < 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        deathPSys.transform.parent = null;
        deathPSys.Play();
    }
}
