using Cinemachine;
using FMOD;
using FMOD.Studio;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    public enum PlayerState { Idle, Walk, Jump, Mine, Drawning, SpellCreate, SpellUp, Dance};
    [HideNormalInspector] public PlayerState playerState = PlayerState.Idle;
    public PlayerState pState { get { return playerState; } set { if(playerState != value) PlayerStateChange(value); } }

    [HideInInspector] public Animator anim;
    [HideInInspector] public CharacterController _characterController;
    PlayerMovement pM;
    public Tile respawnTile;
    [HideInInspector] public Tile tileUnder;
    [HideInInspector] public List<Item> holdableItems;
    [HideNormalInspector] public Item heldItem;
    [HideInInspector] public Item closestItem;

    [HideInInspector] public List<Interactor> interactors;
    public ParticleSystem waterSplash;
    public float throwStrength;
    [Range(-5, 5)]public float throwYAxisDirection;
    [HideInInspector] public Collider col;
    public ParticleSystem hitParticleSystem;
    [HideInInspector] public List<Transform> pointers;
    public float drawningTimer = 2;
    [HideInInspector] public Transform dummyTarget;
    bool respawning;
    float currentGravMult;

    private void Awake()
    {
        GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = Color.black;

        transform.parent = null;
        dummyTarget = transform.Find("DummyTarget");
        if (TileSystem.Instance.isHub && Time.time > 1f) FindObjectOfType<CameraCtr>().AddPlayer(dummyTarget);
        col = GetComponent<Collider>();

        pointers = new List<Transform>();
        Transform pointerFolder = transform.Find("PointerFolder");
        foreach (Transform go in pointerFolder)
        {
            pointers.Add(go);
        }
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        respawnTile = TileSystem.Instance.centerTile;
    }

    private void Start()
    {

        DontDestroyOnLoad(this.gameObject);
        respawnTile = TileSystem.Instance.centerTile;
        interactors = new List<Interactor>();   
        holdableItems = new List<Item>();
        pM = GetComponent<PlayerMovement>();
        _characterController = pM.GetComponent<CharacterController>();
        _characterController.enabled = false;
        transform.position = TileSystem.Instance.centerTile.transform.GetChild(0).position + Vector3.up * 3;
        _characterController.enabled = true;
        anim = GetComponentInChildren<Animator>();
        SceneManager.sceneLoaded += OnLoad;
    }

    private void Update()
    {
        AnimationStatesHandler();
    }

    private void AnimationStatesHandler()
    {
        if (pState == PlayerState.Mine)
        {
            Vector3 pos = Vector3.zero;
            foreach (var inter in interactors)
            {
                pos += inter.transform.position;
            }
            pos /= interactors.Count;
            pos.y = transform.position.y;
            transform.LookAt(pos);
        }
        else if (_characterController.isGrounded && pM._input != Vector2.zero && pState != PlayerState.Drawning && pState != PlayerState.Jump) pState = PlayerState.Walk;
        else if (_characterController.isGrounded && pState != PlayerState.Drawning && pState != PlayerState.Jump) pState = PlayerState.Idle;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(respawning)
        {
            pM.canMove = true;
            pM.gravityMultiplier = currentGravMult;
        }
        if (hit.transform.TryGetComponent<Tile>(out Tile tileO) && hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - tileUnder.transform.position.y <= 3 && hit.transform.position.y - tileUnder.transform.position.y > 1)
        {
            pM.jumpInput = true;
            pState = PlayerState.Jump;
        }
        else if (hit.transform.CompareTag("Water") && pState != PlayerState.Drawning)
        {
            StartCoroutine(Drawning(hit));
        }
        else if (hit.transform.TryGetComponent<PlayerMovement>(out PlayerMovement player) && pM.dashFlag && !player.dashFlag)
        {
            StartCoroutine(player.Dashed(-hit.normal, pM.pushStrength));
        }
    }

    IEnumerator Drawning(ControllerColliderHit hit)
    {
        if (heldItem != null)
        {
            heldItem.GrabRelease(true);
            Destroy(heldItem.gameObject);
        }
        currentGravMult = pM.gravityMultiplier;
        WaterHit(hit);
        yield return new WaitForSeconds(drawningTimer);
        DrawningEnd(hit);
    }

    private void WaterHit(ControllerColliderHit hit)
    {
        pState = PlayerState.Drawning;
        Physics.IgnoreCollision(col, hit.collider, true);
        Instantiate(waterSplash, hit.point + 2 * Vector3.up, Quaternion.identity, null);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Drowning", transform.position);
        dummyTarget.parent = null;
        pM.canMove = false;


        pM._velocity = 0;
        pM.gravityMultiplier = .03f;
    }

    private void DrawningEnd(ControllerColliderHit hit)
    {
        Physics.IgnoreCollision(col, hit.collider, false);
        pM._velocity = 0;
        _characterController.enabled = false;
        transform.position = respawnTile.transform.position + (25f + 3f) * Vector3.up;
        _characterController.enabled = true;
        dummyTarget.parent = transform;
        dummyTarget.localPosition = Vector3.zero;
        respawning = true;
        pState = PlayerState.Idle;

    }

    private void PlayerStateChange(PlayerState value)
    {
        playerState = value;
        anim.Play(playerState.ToString());
    }
}
