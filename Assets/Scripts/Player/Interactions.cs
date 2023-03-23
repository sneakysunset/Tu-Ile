using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Interactions : MonoBehaviour
{
    bool isGrowing;
    [HideInInspector] public bool terraforming;
    [HideInInspector] public bool isMining;
    public float growingSpeed;
    public float hitDistance;
    public float hitRadius;
    public float hitRate;
    [HideNormalInspector] public Interactor interactor;
    private Transform miningZone;
    private RessourcesManager rManager;
    private Player player;

    private void Start()
    {
        miningZone = transform.Find("MiningZone");
        rManager = FindObjectOfType<RessourcesManager>();
        player = GetComponent<Player>();
    }

    public void OnMining(InputAction.CallbackContext context)
    {
/*        if (context.started)
        {
            isMining = true;
            miningZone.gameObject.SetActive(true);
        }
        else if (context.canceled || context.performed)
        {
            isMining = false;
            miningZone.gameObject.SetActive(false);
            if (interactor)
            {
                interactor.OnInteractionExit();
                interactor = null;
            }
        }*/
    }

    public void OnStopDegrading(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isGrowing = true;
            //StartCoroutine(afterPhysics());
        }
        else if (context.canceled || context.performed)
        {
            isGrowing = false;
            terraforming = false;
            if (tile && tile.isGrowing)
            {
                tile.isGrowing = false;
            }
        }
    }

    public void GetRessource(int ressourceNum)
    {
        rManager.wood += ressourceNum;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out interactor))
        {
            if (interactor.interactable && !player.heldItem)
            {
                isMining = true;
                interactor.OnInteractionEnter(hitRate, this);
            }
        }
    }   

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out interactor))
        {
            interactor.OnInteractionExit();
            isMining = false;
            interactor = null;
        }
        if(other.TryGetComponent<Tile>(out Tile otherTile) && otherTile.isGrowing)
        {
            otherTile.isGrowing = false;
            isGrowing = false;
            terraforming = false;
        }
    }


    Tile tile;
        
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("DegradingTile") && hit.normal.y > .9f && isGrowing)
        {
            tile = hit.gameObject.GetComponent<Tile>();
            if (!tile.isGrowing && tile.currentPos != tile.ogPos)
            {
                terraforming = true;
                tile.currentPos.y += tile.heightByTile;
                tile.isGrowing = true;
            }
            else
            {
                terraforming = false;
            }

/*            if (tile.currentPos == tile.ogPos)
            {
                tile.timer = Random.Range(tile.minTimer, tile.maxTimer);
                tile.isDegrading = false;
                tile.transform.tag = "Tile";
            }*/
        }
    }

    WaitForFixedUpdate waiter;
    IEnumerator afterPhysics()
    {
        yield return waiter;
        isGrowing = false;
    }
}
