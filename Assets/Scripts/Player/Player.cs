using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    Animator anim;
    CharacterController _characterController;
    PlayerMovement pM;
    TileSelector tileSelec;
    Interactions inter;
    [HideInInspector] public EventInstance movingSound;
    public Tile respawnTile;
    [HideInInspector] public List<Item> holdableItems;
    [HideInInspector] public List<Item> heldItems;
    [HideInInspector] public Item closestItem;

    private void Start()
    {
        holdableItems = new List<Item>();
        heldItems = new List<Item>();
        pM = GetComponent<PlayerMovement>();
        inter = GetComponent<Interactions>();
        _characterController = pM.GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        tileSelec = GetComponent<TileSelector>();
    }

    private void Update()
    {
        if(_characterController.isGrounded && inter.terraforming)
        {
            anim.Play("Terraform", 0);
        }
        else if(_characterController.isGrounded && inter.isMining)
        {
            anim.Play("Mine", 0);
        }
        else if (_characterController.isGrounded && pM._input != Vector2.zero)
        {
            if (!pM.moveFlag)
            {
                pM.moveFlag = true;
                movingSound.start();
            }
            anim.Play("Walk", 0);
        }
        else if ((!_characterController.isGrounded || pM._input == Vector2.zero) && pM.moveFlag)
        {
            pM.moveFlag = false;
            movingSound.stop(STOP_MODE.ALLOWFADEOUT);
            if (_characterController.isGrounded)
            {
                anim.Play("Idle", 0);
            }
        }
        else
        {
            if (_characterController.isGrounded)
            {
                anim.Play("Idle", 0);
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.TryGetComponent<Tile>(out Tile tileO) && hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - tileSelec.tileUnder.transform.position.y <= 3 && hit.transform.position.y - tileSelec.tileUnder.transform.position.y > 1)
        {
            pM.jumpInput = true;
            anim.Play("Jump", 0);
        }
        else if (hit.transform.CompareTag("Water"))
        {
            transform.position = respawnTile.transform.position + 25f * Vector3.up;
        }
        else if (hit.transform.TryGetComponent<PlayerMovement>(out PlayerMovement player) && pM.dashFlag && !player.dashFlag)
        {
            StartCoroutine(player.Dashed(-hit.normal, pM.pushStrength));
        }
    }

    private void OnDestroy()
    {
        movingSound.stop(STOP_MODE.IMMEDIATE);
        movingSound.release();
    }
}
