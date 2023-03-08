using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionReaction : MonoBehaviour
{
    public VieBateau vie;
    public float bonusDePointDeVie = 10f;
    public float tempsDeRecuperation = 10f;
    public float tempsDeCanalisation = 2f;
    bool jsuisdedan;
    bool enRecuperation;
    float n;
    float t;
    public MeshRenderer rendu;

    public GameObject UI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name +" est rentré");
            jsuisdedan = true;
            if (!enRecuperation)
            {
                UI.SetActive(true);
            }
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
            UI.SetActive(false);
        }    
    }
    private void Update()
    {
        if (Input.GetButton("Jump") && !enRecuperation && jsuisdedan)
        {
            t += Time.deltaTime;
            if(t>= tempsDeCanalisation)
            {
                t = 0f;
                vie.pointDeVie += bonusDePointDeVie;
                enRecuperation = true;
                rendu.material.color = Color.red;
            }
            UI.GetComponent<UIMaintientAction>().UpdateInfoAction(tempsDeCanalisation, t);
        }
        else
        {
            t = 0f;
        }

        //En Recuperation
        if (enRecuperation)
        {
            UI.SetActive(false);

            n += Time.deltaTime;
            if(n>= tempsDeRecuperation)
            {
                enRecuperation = false;
                n = 0f;
                rendu.material.color = Color.blue;
            }
        }
    }
}
