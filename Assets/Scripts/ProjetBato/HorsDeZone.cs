using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorsDeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bato"))
        {
            print("Perdu");
        }
    }
}
