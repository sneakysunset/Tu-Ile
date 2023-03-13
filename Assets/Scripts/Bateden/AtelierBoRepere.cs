using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtelierBoRepere : MonoBehaviour
{
    public bool dansBoRepere;
    public VieBateau vie;
    public Recuperation recuperation;
    public Transform pointDePose;
    bool enActivation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name +" est rentré");
            dansBoRepere = true;
            /*if (!enRecuperation)
            {
                UI.SetActive(true);
            }*/
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug
            //print(other.name + " est sortie");
            dansBoRepere = false;
            //UI.SetActive(false);
        }
    }
    private void Update()
    {
        if (dansBoRepere && Input.GetButtonDown("Jump"))
        {
            /*recuperation.jeTeTiens = false;
            recuperation.IRecuperable.transform.SetParent(pointDePose);
            recuperation.IRecuperable.transform.position = pointDePose.transform.position;
            enActivation = true;*/
            Destroy(recuperation.IRecuperable);
            
        }
        if (enActivation)
        {

        }
    }
}
