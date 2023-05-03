using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Mining : MonoBehaviour
{
    public float hitRate;
    private Player player;
    public GameObject axe;
    
    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        InteractorChange();
        
        player.anim.SetFloat("MiningSpeed", hitRate);
    }

    private void InteractorChange()
    {
/*        if (player.interactors.Count > 1)
        {
            float dist = Mathf.Infinity;
            Interactor tempInte = null;
            foreach (Interactor inte in player.interactors)
            {
                float tempDist = Vector3.Distance(transform.position, inte.transform.position);
                if (dist < tempDist)
                {
                    dist = tempDist;
                    tempInte = inte;
                }
            }
            if (player.interactor == null)
            {
                player.interactor = tempInte;
                player.isMining = true;
                player.interactor.OnInteractionEnter(player);
                axe.SetActive(true);
            }
            else if (tempInte != player.interactor)
            {
                player.interactor.OnInteractionExit(player);
                player.interactor = tempInte;
                player.interactor.OnInteractionEnter(player);
            }

        }
        else if (player.interactors.Count == 1 && player.interactor == null)
        {
            player.interactor = player.interactors[0];
            player.isMining = true;
            player.interactor.OnInteractionEnter(player);
            axe.SetActive(true);
        }
        else if (player.interactors.Count == 0 && player.interactor != null)
        {
            player.isMining = false;
            player.GetComponent<Player_Mining>().axe.SetActive(false);
            player.interactor = null;
        }*/
/*        if(player.interactors.Count > 0)
        {
            foreach(var interactor in player.interactors)
            {
                interactor.
            }
        }*/

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out Interactor tempInteractor))
        {
            if (tempInteractor.interactable && !player.interactors.Contains(tempInteractor) && !player.heldItem)
            {
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
            }
            //player.interactor.OnInteractionExit(player);
        }
    }
}
