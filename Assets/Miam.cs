using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Miam : MonoBehaviour
{
    public VieBateau vie;
    public Recuperation recuperation;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BoRepere") && Input.GetButtonDown("Jump"))
        {
            print("Jo");
            vie.pointDeVie += 10f;
            Destroy(gameObject);
        }
    }
}
