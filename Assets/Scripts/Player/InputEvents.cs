using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputEvents : MonoBehaviour
{
    Tile selectedTile;
    public LayerMask hitMask;
    private bool smalling, growing, bumping;

    #region Methods
    private void Update()
    {
        RayCasting();
        bumping = false;

    }
    #endregion

    #region TileCast
    private void RayCasting()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (selectedTile && selectedTile.gameObject != hit.transform.gameObject)
            {
                selectedTile.isSelected = false;
                FMODUnity.RuntimeManager.PlayOneShot("event:/plok");
            }
            selectedTile = hit.transform.GetComponent<Tile>();
            selectedTile.OnSelected();
            TileFunctions();
        }
    }

    private void TileFunctions()
    {
        if (growing)
        {
            transform.position += Mathf.Clamp(Time.deltaTime * selectedTile.maxVelocity, 0, transform.localScale.y) / transform.localScale.y * Vector3.up;
            growing = false;
        }
        
        if (smalling)
        {
            transform.position -= Mathf.Clamp(Time.deltaTime * selectedTile.maxVelocity, 0, transform.localScale.y) / transform.localScale.y * Vector3.up;
            smalling = false;
        }

        if (bumping)
        {
            selectedTile.tileB.Bump();
        }
    }
    #endregion

    #region Inputs

    public void OnSmalling(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            smalling = true;
        }
        else if(cbx.canceled || cbx.performed)
        {
            smalling = false;
        }
    }

    public void OnGrowing(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            growing = true;
        }
        else if (cbx.canceled || cbx.performed)
        {
            growing = false;
        }
    }

    public void OnBumping(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            bumping = true;
        }
    }

#endregion
}
