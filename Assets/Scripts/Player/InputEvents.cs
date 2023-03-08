using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using FMODUnity;
public class InputEvents : MonoBehaviour
{
    [HideInInspector] public Tile selectedTile;
    public LayerMask hitMask;
    private bool smalling, growing, bumping;
    [HideInInspector] public EventInstance terraformingSound;


    #region Methods

    private void Start()
    {
        terraformingSound = FMODUnity.RuntimeManager.CreateInstance("event:/monte");
    }

    private void Update()
    {
        RayCasting();
        bumping = false;

    }

    private void OnDestroy()
    {
        // Release the FMOD event instance when the object is destroyed
        terraformingSound.release();
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

            selectedTile.currentPos += Mathf.Clamp(Time.deltaTime * selectedTile.maxVelocity, 0, selectedTile.transform.localScale.y) / selectedTile.transform.localScale.y * Vector3.up;
        }
        
        if (smalling)
        {
            selectedTile.currentPos -= Mathf.Clamp(Time.deltaTime * selectedTile.maxVelocity, 0, selectedTile.transform.localScale.y) / selectedTile.transform.localScale.y * Vector3.up;
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
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.position = RuntimeUtils.ToFMODVector(selectedTile.transform.position + 45 * Vector3.up);
            terraformingSound.set3DAttributes(attributes);
            terraformingSound.start();
        }
        else if(cbx.canceled || cbx.performed)
        {
            terraformingSound.setParameterByName("Release Time", 50000);
            terraformingSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            smalling = false;
        }
    }

    public void OnGrowing(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.position = RuntimeUtils.ToFMODVector(selectedTile.transform.position + 45 * Vector3.up);
            terraformingSound.set3DAttributes(attributes);
            terraformingSound.start();

            growing = true;
        }
        else if (cbx.canceled || cbx.performed)
        {
            terraformingSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

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

    #region Sound
    #endregion
}
