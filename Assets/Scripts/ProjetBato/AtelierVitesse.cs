using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtelierVitesse : MonoBehaviour
{
    public BatoMovement batoMovement;
    public float speedIncrease = 0.4f;
    private bool isInZone;
    float t;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInZone = true;
        }
    }
    private void Update()
    {
        if(isInZone&& Input.GetButton("Jump") && batoMovement.isMoving)
        {
            batoMovement.decrease = 0;
            t += Time.deltaTime;
            if (t >= 1)
            {
                batoMovement.speed += speedIncrease;
                t = 0f;
            }
        }
        else
        {
            batoMovement.decrease = batoMovement.refDecrease;
        }
    }
}
