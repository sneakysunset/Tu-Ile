using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Interactions : MonoBehaviour
{
    bool isGrowing;
    bool isMining;
    public float growingSpeed;
    public float hitDistance;
    public float hitRadius;
    public float hitRate;
    [HideNormalInspector] public Interactor interactor;
    private Transform miningZone;

    private void Start()
    {
        miningZone = transform.Find("MiningZone");    
    }

    public void OnMining(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isMining = true;
            miningZone.gameObject.SetActive(true);
        }
        else if(context.canceled || context.performed)
        {
            isMining = false;
            miningZone.gameObject.SetActive(false);
            if (interactor)
            {
                interactor.OnInteractionExit();
                interactor = null;
            }
        }
    }

    public void OnStopDegrading(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isGrowing = true;
            StartCoroutine(afterPhysics());
        }
        /*        else if(context.canceled || context.performed)
                {
                    isGrowing = false;
                }*/
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out interactor))
        {
            if (interactor.interactable)
            {
                interactor.OnInteractionEnter(hitRate, this);
            }
        }
    }   

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Interactor") && other.transform.TryGetComponent<Interactor>(out interactor))
        {
            interactor.OnInteractionExit();
            interactor = null;
        }
    }



    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("DegradingTile") && hit.normal.y > .9f && isGrowing)
        {
            Tile tile = hit.gameObject.GetComponent<Tile>();
            if (!tile.isGrowing && tile.currentPos != tile.ogPos)
            {
                tile.currentPos.y += tile.heightByTile;
                tile.isGrowing = true;
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
