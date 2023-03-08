using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Recuperation : MonoBehaviour
{
    bool jsuisdedans;
    public Collider IRecuperable;
    public Transform pointDeRecuperation;
    public Transform bacAJouet;
    public bool saCommence;
    public bool jeTeTiens;
    public AtelierBoRepere atelierBo;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Recuperable"))
        {
            //Debug
            //print(other.name +" est rentré");
            jsuisdedans = true;
            IRecuperable = other;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Recuperable"))
        {
            //Debug
            //print(other.name + " est sorti");
            jsuisdedans = false;
        }
    }
    private void Update()
    {
        if (jsuisdedans && saCommence && !jeTeTiens)
        {
            IRecuperable.transform.SetParent(pointDeRecuperation);
            IRecuperable.transform.position = pointDeRecuperation.position;
            IRecuperable.attachedRigidbody.isKinematic = true;
            jeTeTiens = true;
        }
        else if(jeTeTiens && saCommence && !atelierBo.dansBoRepere)
        {
            IRecuperable.transform.SetParent(bacAJouet);
            IRecuperable.attachedRigidbody.isKinematic = false;
            jeTeTiens = false;
        }
        if (saCommence)
        {
            saCommence = false;
        }
    }
    public void OnRecuperation(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            saCommence = true;
        }
    }
}
