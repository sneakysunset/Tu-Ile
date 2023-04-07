using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Mining : MonoBehaviour
{
    public float hitDistance;
    public float hitRadius;
    public float hitRate;
    private Player player;
    public GameObject axe;
    
    private void Start()
    {
        player = GetComponent<Player>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out player.interactor))
        {
            if (player.interactor.interactable && !player.heldItem)
            {
                player.isMining = true;
                axe.SetActive(true);
                player.interactor.OnInteractionEnter(hitRate, player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out player.interactor))
        {
            player.interactor.OnInteractionExit();
            axe.SetActive(false);
            player.isMining = false;
            player.interactor = null;
        }
    }
}
