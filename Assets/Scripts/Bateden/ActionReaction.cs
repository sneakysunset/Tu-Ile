using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionReaction : MonoBehaviour
{
    public VieBateau vie;
    public float bonusDePointDeVie = 10f;
    public float tempsDeRecuperation = 10f;
    bool jsuisdedan;
    bool enRecuperation;
    float t;
    float n;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name +" est rentré");
            jsuisdedan = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name + " est dedans");

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name + " est sortie");
            jsuisdedan = false;
        }    
    }
    private void Update()
    {
        if (jsuisdedan)
        {
            //faire UI input maintenue pour un event ponctuelle
            if (Input.GetButtonDown("Jump") && !enRecuperation)
            {
                vie.pointDeVie += bonusDePointDeVie;
                enRecuperation = true;
            }
        }

        //En Recuperation
        if (enRecuperation)
        {
            n += Time.deltaTime;
            if(n>= tempsDeRecuperation)
            {
                enRecuperation = false;
            }
        }
    }
}
