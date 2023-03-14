using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    LineRenderer lineR;
    LoadScene lS;
    FMOD.Studio.EventInstance laserSound;
    private void Start()
    {
        lS = FindObjectOfType<LoadScene>();
        lineR = GetComponent<LineRenderer>();
        lineR.positionCount = 2;    
        // laserSound = FMODUnity.RuntimeManager.CreateInstance("event:/Tile/Blockld/laser");
        // laserSound.start();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore))
        {
            lineR.SetPosition(0, transform.position);
            lineR.SetPosition(1, hit.point);
            if (hit.transform.CompareTag("Player"))
            {
                lS.ReloadScene();
            }
        }
        else
        {
            lineR.SetPosition(0, transform.position);
            lineR.SetPosition(1, transform.forward * 1000);
        }
    }
}
