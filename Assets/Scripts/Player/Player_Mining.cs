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
        numInt = player.interactors.Count;

        if(player.interactors.Count > 0)player.anim.SetFloat("MiningSpeed", hitRate * player.interactors[0].hitRateSpeedModifier);
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out Interactor tempInteractor))
        {
            if (tempInteractor.interactable && !player.interactors.Contains(tempInteractor) && !player.heldItem && player.pState != Player.PlayerState.SpellUp && player.pState != Player.PlayerState.SpellCreate)
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
        }
    }
}
