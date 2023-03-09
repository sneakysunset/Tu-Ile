using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using FMODUnity;
public class InputEvents : MonoBehaviour
{
    [HideInInspector] public Tile selectedTile;
    private TileSystem tileS;
    public LayerMask hitMask;
    private bool smalling, growing, bumping;
    [HideInInspector] public EventInstance terraformingSound;
    public bool controller;
    private Vector2 moveValue;
    public float timerToChangeTile = .3f;
    private float timer;
    #region Methods

    private void Start()
    {
        terraformingSound = FMODUnity.RuntimeManager.CreateInstance("event:/monte");
        tileS = FindObjectOfType<TileSystem>();
        timer = timerToChangeTile;
        selectedTile.OnSelected();
    }

    private void Update()
    {
        if (moveValue == Vector2.zero) timer = 0;
        if (!controller) RayCasting();
        else
        {
            Vector2Int tileCoord = CalculateTileCoordinates(new Vector2Int(selectedTile.coordX, selectedTile.coordY));
            TileControllerMove(tileCoord);
        }
        bumping = false;
    }

    private void OnDestroy()
    {
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
                selectedTile = hit.transform.GetComponent<Tile>();
                selectedTile.OnSelected();
            }
            print(1);
            TileFunctions();
        }
    }

    private Vector2Int CalculateTileCoordinates(Vector2Int currentTileCoor)
    {
        timer -= Time.deltaTime;    
        selectedTile.gameObject.layer = 2;
        RaycastHit hit;
        //Debug.DrawRay(selectedTile.transform.position + Vector3.up * 40, new Vector3(moveValue.x, 0, moveValue.y) * 150, Color.blue, Time.deltaTime);
        if(timer < 0 && Physics.Raycast(new Vector3(selectedTile.transform.position.x, 40, selectedTile.transform.position.z), new Vector3(moveValue.x, 0, moveValue.y), out hit, 150, hitMask, QueryTriggerInteraction.Ignore))
        {
            //print(hit.transform.name);
            Tile tile = hit.transform.GetComponent<Tile>();
            Vector2Int newTileCoord = new Vector2Int(tile.coordX, tile.coordY);
            if (tileS.tiles[newTileCoord.x, newTileCoord.y].walkable)
            {
                timer = timerToChangeTile;
                selectedTile.gameObject.layer = LayerMask.NameToLayer("Tile");
                return newTileCoord;
            }
        }
        selectedTile.gameObject.layer = LayerMask.NameToLayer("Tile");
        return currentTileCoor;
    }

    private void TileControllerMove(Vector2Int tileCoord)
    {
        if(selectedTile.coordX != tileCoord.x || selectedTile.coordY != tileCoord.y)
        {
            selectedTile.isSelected = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/plok");
            selectedTile = tileS.tiles[tileCoord.x, tileCoord.y];
            selectedTile.OnSelected();
        }
        TileFunctions();
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

    public void OnMoving(InputAction.CallbackContext cbx)
    {
        var v = cbx.ReadValue<Vector2>();
        float cameraAngle = -Camera.main.transform.rotation.eulerAngles.y;
        moveValue = Rotate(v, cameraAngle);
        //print(cameraAngle + "  et  " + v + "  et  " +  moveValue);
        Debug.DrawRay(selectedTile.transform.position + 45 * Vector3.up, new Vector3(moveValue.x, 0, moveValue.y), Color.black, Time.deltaTime);

    }
    #endregion
    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    #region Sound
    #endregion
}
