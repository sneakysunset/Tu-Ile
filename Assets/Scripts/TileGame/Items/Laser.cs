using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    LineRenderer lineR;
    LoadScene lS;
    private void Start()
    {
        lS = FindObjectOfType<LoadScene>();
        lineR = GetComponent<LineRenderer>();
        lineR.positionCount = 2;
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
