using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    Animator anim;
    [HideInInspector] public CharacterController _characterController;
    PlayerMovement pM;
    TileSelector tileSelec;
    Interactions inter;
    [HideInInspector] public EventInstance movingSound;
    public Tile respawnTile;
    [HideInInspector] public Tile tileUnder;
    private TileSystem tileS;
    [HideInInspector] public List<Item> holdableItems;
    [HideInInspector] public Item heldItem;
    [HideInInspector] public Item closestItem;
    [HideInInspector] public bool isMining;
    [HideInInspector] public Interactor interactor;
    public ParticleSystem waterSplash;
    bool waterValidate;

    private void Awake()
    {
        if (respawnTile == null)
        {
            foreach (Tile til in tileS.tiles)
            {
                if (til.walkable && !til.degradable)
                {
                    respawnTile = til; break;
                }
            }
            transform.position = respawnTile.transform.position + Vector3.up * 22.5f;
        }
        FindObjectOfType<CameraCtr>().AddPlayer(transform);
    }

    private void Start()
    {
        tileS = FindObjectOfType<TileSystem>();
        holdableItems = new List<Item>();
        pM = GetComponent<PlayerMovement>();
        _characterController = pM.GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        tileSelec = GetComponent<TileSelector>();

    }

    private void Update()
    {
        tileUnder = tileS.WorldPosToTile(transform.position);

/*        if(_characterController.isGrounded && inter.terraforming)
        {
            anim.Play("Terraform", 0);
        }
        else*/ if(_characterController.isGrounded && isMining)
        {
            anim.Play("Mine", 0);
            Vector3 pos = interactor.transform.position;
            pos.y = transform.position.y;
            transform.LookAt(pos);
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

        if(heldItem && heldItem.GetType() == typeof(Item_Stack))
        {
            Item_Stack stack = heldItem as Item_Stack;
            if(stack.numberStacked == 0)
            {
                Destroy(heldItem.gameObject);
                heldItem = null;
            }
        }
        waterValidate = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.TryGetComponent<Tile>(out Tile tileO) && hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - tileSelec.tileUnder.transform.position.y <= 3 && hit.transform.position.y - tileSelec.tileUnder.transform.position.y > 1)
        {
            pM.jumpInput = true;
            anim.Play("Jump", 0);
        }
        else if (hit.transform.CompareTag("Water") && !waterValidate)
        {
            waterValidate = true;
            Instantiate(waterSplash, hit.point + 2 * Vector3.up, Quaternion.identity, null);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Water_fall");
            transform.position = respawnTile.transform.position + 25f * Vector3.up;
            if (heldItem != null)
            {
                heldItem.GrabRelease(this);
                Destroy(heldItem.gameObject);
                heldItem = null;
            }
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
