using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Mining : MonoBehaviour
{
    public float hitRate;
    private Player player;
    public GameObject axe;
    int numInt;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if(player.interactors.Count == 0 && player.interactors.Count != numInt)
        {
            axe.SetActive(false);
        }
        numInt = player.interactors.Count;

        player.anim.SetFloat("MiningSpeed", hitRate * player.interactors[0].hitRateSpeedModifier);
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out Interactor tempInteractor))
        {
            if (tempInteractor.interactable && !player.interactors.Contains(tempInteractor) && !player.heldItem)
            {
                if(player.interactors.Count == 0)
                {
                    axe.SetActive(true);
                }
                tempInteractor.OnInteractionEnter(player);
                player.interactors.Add(tempInteractor);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out Interactor tempInt))
        {
            if (player.interactors.Contains(tempInt))
            {
                tempInt.OnInteractionExit(player);
                if(player.interactors.Count == 0) { axe.SetActive(false); }
            }
        }
    }
}
