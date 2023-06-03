using Cinemachine;
using FMOD;
using FMOD.Studio;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum PlayerState { Idle, Walk, Jump, Mine, Drawning, SpellCreate, SpellUp, Dance };
    [HideNormalInspector] public PlayerState playerState = PlayerState.Idle;
    public PlayerState pState { get { return playerState; } set { if (playerState != value) PlayerStateChange(value); } }

    [HideInInspector] public Animator anim;
    private Player_Pause pPause;
    [HideInInspector] public CharacterController _characterController;
    [HideInInspector] public PlayerMovement pM;
    [HideInInspector] public Player_Mining pMin;
    public Tile respawnTile;
    [HideInInspector] public Tile tileUnder;
    [HideNormalInspector] public List<Item> holdableItems;
    [HideNormalInspector] public Item heldItem;
    [HideNormalInspector] public Item closestItem;

    [HideNormalInspector] public List<Interactor> interactors;
    public ParticleSystem waterSplash;
    public float throwStrength;
    [Range(-5, 5)] public float throwYAxisDirection;
    [HideInInspector] public Collider col;
    public ParticleSystem hitParticleSystem;
    [HideInInspector] public List<Transform> pointers;
    public float drawningTimer = 2;
    [HideInInspector] public Transform dummyTarget;
    bool respawning;
    float currentGravMult;
    CameraCtr cam;
    ChainIKConstraint[] iks;
    public Transform itemParent1, itemParent2;
    [HideNormalInspector] public Vector2Int previousScenePos;
    [HideNormalInspector] public Ship_CharacterController ship;
    [HideNormalInspector] public bool isShipped;
    [HideNormalInspector] public int playerIndex;
    [HideNormalInspector] public CinemachineVirtualCamera closeUpCam;
    [HideNormalInspector] public Transform myCamera;
    [HideNormalInspector] public string groundType;
    private FMOD.Studio.EventInstance danceEvent;

    private void Awake()
    {
        GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = Color.black;
        closeUpCam = GetComponentInChildren<CinemachineVirtualCamera>();
        pPause = GetComponent<Player_Pause>();
        transform.parent = null;
        dummyTarget = transform.Find("DummyTarget");
        if (/*TileSystem.Instance.isHub && */Time.time > .1f) FindObjectOfType<CameraCtr>().AddPlayer(dummyTarget, this);
        col = GetComponent<Collider>();
        pointers = new List<Transform>();
        Transform pointerFolder = transform.Find("PointerFolder");
        foreach (Transform go in pointerFolder)
        {
            pointers.Add(go);
        }
        iks = GetComponentsInChildren<ChainIKConstraint>();
        Player_Pause.pauseMenuActivation += OnPause;
        Player_Pause.pauseMenuDesactivation += OnUnPause;
    }

    void OnPause()
    {
        closeUpCam.Priority = 10;
        FMODUtils.SetFMODEvent(ref danceEvent, "event:/Tuile/Character/Voix/Beatbox", transform);
    }

    void OnUnPause()
    {
        closeUpCam.Priority = 0;
        FMODUtils.StopFMODEvent(ref danceEvent, true);
    }

    public void OnLoad()
    {
        respawnTile = TileSystem.Instance.centerTile;
        pState = PlayerState.Idle;

        float x = TileSystem.Instance.centerTile.transform.position.x - TileSystem.Instance.previousCenterTile.transform.position.x;
        float z = TileSystem.Instance.centerTile.transform.position.z - TileSystem.Instance.previousCenterTile.transform.position.z;
        Vector3 diff = new Vector3(x, 0, z);
        Vector3 pos = TileSystem.Instance.centerTile.transform.position;
        pos.y = TileSystem.Instance.previousCenterTile.transform.position.y;
        TileSystem.Instance.previousCenterTile.transform.position -= 15 * Vector3.up;
        TileSystem.Instance.centerTile.transform.position = pos;
        _characterController.enabled = false;
        transform.position += diff;
        _characterController.enabled = true;
        if(heldItem != null )
        {
            ObjectPooling.SharedInstance.RemovePoolItem(0, heldItem.gameObject, heldItem.GetType().ToString());
            heldItem = null;
        }

    }

    private void Start()
    {
        respawnTile = TileSystem.Instance.centerTile;
        interactors = new List<Interactor>();
        holdableItems = new List<Item>();
        pM = GetComponent<PlayerMovement>();
        pMin = GetComponent<Player_Mining>();
        _characterController = pM.GetComponent<CharacterController>();
        _characterController.enabled = false;
        transform.position = TileSystem.Instance.centerTile.minableItems.GetChild(playerIndex).position + Vector3.up * 3.5f;
        _characterController.enabled = true;
        anim = GetComponentInChildren<Animator>();
        GridUtils.onLevelMapLoad += OnLoad;
        GridUtils.onEndLevel += OnEndLevel;
        currentGravMult = pM.gravityMultiplier;
    }

    void OnEndLevel(Tile tile) => respawnTile = tile;

    private void Update()
    {
        AnimationStatesHandler();

        if (heldItem != null && pState != PlayerState.SpellCreate && pState != PlayerState.SpellUp && pState != PlayerState.Jump)
        {
            foreach (ChainIKConstraint ik in iks)
            {
                ik.weight = 1;
            }
            if(heldItem.transform.parent == itemParent2)
            {
                heldItem.transform.parent = itemParent1;
                heldItem.transform.position = itemParent1.position;
                heldItem.transform.rotation = itemParent1.rotation;
                heldItem.transform.Rotate(0, 90, 0);
            }
        }
        else
        {
            foreach (ChainIKConstraint ik in iks)
            {
                ik.weight = 0;
            }
            if(heldItem != null && heldItem.transform.parent == itemParent1)
            {
                heldItem.transform.parent = itemParent2;
                heldItem.transform.position = itemParent2.position;
                heldItem.transform.rotation = itemParent1.rotation;
                heldItem.transform.Rotate(0, 90, 0);
            }
        }

        if (isShipped) _characterController.enabled = false;

    }

    private void OnDisable()
    {
        GridUtils.onLevelMapLoad -= OnLoad;
        GridUtils.onEndLevel -= OnEndLevel;
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
        else if (pPause.isPaused && (pState == PlayerState.Idle || pState == PlayerState.Walk))
        {
            Vector3 vec = myCamera.position;
            vec.y = transform.position.y;
            transform.LookAt(vec);
            pState = PlayerState.Dance;
        }
        else if (_characterController.isGrounded && pM._input != Vector2.zero && (pState == PlayerState.Idle || pState == PlayerState.Dance)) pState = PlayerState.Walk;
        else if (_characterController.isGrounded && pM._input == Vector2.zero && (pState == PlayerState.Walk || pState == PlayerState.Dance)) pState = PlayerState.Idle;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(respawning)
        {
            pM.canMove = true;
            pM.gravityMultiplier = currentGravMult;
            respawning = false;
        }
        if (pM.autoJump && hit.transform.TryGetComponent<Tile>(out Tile tileO) && hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - tileUnder.transform.position.y <= 3 && hit.transform.position.y - tileUnder.transform.position.y > 1)
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
        if (!_characterController.isGrounded)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Stomp", transform.position);
        }
    }

    IEnumerator Drawning(ControllerColliderHit hit)
    {
        if (heldItem != null)
        {
            Item tempItem = heldItem;
            heldItem.GrabRelease(true);
            //Destroy(heldItem.gameObject);
            ObjectPooling.SharedInstance.RemovePoolItem(0, tempItem.gameObject, tempItem.GetType().ToString());

        }
        WaterHit(hit);
        //currentGravMult = pM.gravityMultiplier;
        yield return new WaitForSeconds(drawningTimer);
        DrawningEnd(hit);
    }

    private void WaterHit(ControllerColliderHit hit)
    {
        pState = PlayerState.Drawning;
        if (pM.stunCor != null)
        {
            StopCoroutine(pM.stunCor);
            pM.UnStun();
        }
        Physics.IgnoreCollision(col, hit.collider, true);
        Instantiate(waterSplash, hit.point + 2 * Vector3.up, Quaternion.identity, null);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Drowning", transform.position);
        dummyTarget.parent = null;
        pM.canMove = false;
        transform.LookAt(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z));

        pM._velocity = 0;
        pM.gravityMultiplier = .03f;
    }

    private void DrawningEnd(ControllerColliderHit hit)
    {
        Physics.IgnoreCollision(col, hit.collider, false);
        pM._velocity = 0;
        _characterController.enabled = false;
        transform.position = respawnTile.minableItems.GetChild(playerIndex).position + 3.5f * Vector3.up;
        _characterController.enabled = true;
        dummyTarget.parent = transform;
        dummyTarget.localPosition = Vector3.zero;
        respawning = true;
        pState = PlayerState.Idle;

    }

    private void PlayerStateChange(PlayerState value)
    {
        if (playerState == PlayerState.Mine) pMin.axe.SetActive(false);
        playerState = value;
        if (playerState == PlayerState.Mine) pMin.axe.SetActive(true);
        if (value == PlayerState.Dance) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        else anim.updateMode = AnimatorUpdateMode.Normal;
        anim.Play(playerState.ToString());
    }

    public IEnumerator Casting(PlayerState _pState)
    {
        pM.canMove = false;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Cast", transform.position);
        pState = _pState;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length - 1);
        pM.canMove = true;
        playerState = PlayerState.Idle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ShipTrigger") && other.transform.parent.TryGetComponent<Ship_CharacterController>(out ship))
        {

        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ShipTrigger") && ship != null && !isShipped )
        {
            ship = null;
        }

    }

    public void OnInteractionInput(InputAction.CallbackContext context)
    {
        if (context.started && ship != null && !isShipped)
        {
            isShipped = true;
            pM.canMove = false;
            Physics.IgnoreCollision(col, ship.col, true);
            _characterController.enabled = false;
            transform.position = ship.holdPoint.position;
            transform.rotation = ship.holdPoint.rotation;
            transform.parent = ship.holdPoint;
            ship.isUsed = true;
            pState = PlayerState.Drawning;
        }
        else if (context.started && isShipped)
        {
            Physics.IgnoreCollision(col, ship.col, false);
            _characterController.enabled = true;
            pM.canMove = true;
            isShipped = false;
            ship.isUsed = false;
            transform.parent = null;
        }
    }
}
