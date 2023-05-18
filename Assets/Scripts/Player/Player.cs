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
    [HideInInspector] public Animator anim;
    [HideInInspector] public CharacterController _characterController;
    PlayerMovement pM;
    TileSelector tileSelec;
    [HideInInspector] public EventInstance movingSound;
    public Tile respawnTile;
    [HideInInspector] public Tile tileUnder;
    private TileSystem tileS;
    [HideInInspector] public List<Item> holdableItems;
    [HideNormalInspector] public Item heldItem;
    [HideInInspector] public Item closestItem;
    [HideInInspector] public bool isMining;
    [HideInInspector] public List<Interactor> interactors;
    public ParticleSystem waterSplash;
    bool waterValidate;
    public float throwStrength;
    [Range(-5, 5)]public float throwYAxisDirection;
    [HideInInspector] public Collider col;
    public ParticleSystem hitParticleSystem;
    [HideInInspector] public List<Transform> pointers;
    public float drawningTimer = 2;
    bool waterFlag = false;
    CinemachineTargetGroup targetGroup;
    SplitScreenEffect sse;
    [HideNormalInspector] public bool isDrawning;
    [HideInInspector] public Transform dummyTarget;

    private void Awake()
    {

        if (respawnTile == null)
        {
            respawnTile = TileSystem.Instance.centerTile;
        }
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
        tileS = TileSystem.Instance;
        respawnTile = tileS.centerTile;
        //transform.position = tileS.centerTile.transform.position + Vector3.up * (GameConstant.tileHeight + 10);
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        tileS = TileSystem.Instance;
        respawnTile = tileS.centerTile;
        interactors = new List<Interactor>();   
        holdableItems = new List<Item>();
        pM = GetComponent<PlayerMovement>();
        _characterController = pM.GetComponent<CharacterController>();
        _characterController.enabled = false;
        transform.position = new Vector3(TileSystem.Instance.centerTile.transform.position.x, GameConstant.tileHeight + 10, TileSystem.Instance.centerTile.transform.position.z);
        _characterController.enabled = true ;
        anim = GetComponentInChildren<Animator>();
        tileSelec = GetComponent<TileSelector>();
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        sse = Camera.main.GetComponent<SplitScreenEffect>();
        SceneManager.sceneLoaded += OnLoad;
    }

    private void Update()
    {
        if(tileUnder != null)
        {
            tileUnder.sand_WalkedOnto = false;
        }
        tileUnder = tileS.WorldPosToTile(transform.position);
        tileUnder.sand_WalkedOnto = true;


        if(isDrawning)
        {
            anim.Play("Drawning");
        }
        if(_characterController.isGrounded && isMining)
        {
            anim.Play("Mine", 0);
            Vector3 pos = Vector3.zero;
            foreach(var inter in interactors)
            {
                pos += inter.transform.position;
            }
            pos /= interactors.Count;
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
        else if (hit.transform.CompareTag("Water") && !waterValidate && !waterFlag)
        {
            StartCoroutine(Drawning(hit));
            
        }
        else if (hit.transform.TryGetComponent<PlayerMovement>(out PlayerMovement player) && pM.dashFlag && !player.dashFlag)
        {
            StartCoroutine(player.Dashed(-hit.normal, pM.pushStrength));
        }
    }

    private void OnTileChange()
    {

    }

    IEnumerator Drawning(ControllerColliderHit hit)
    {
        waterValidate = true;
        waterFlag = true;
        isDrawning = true;
        Physics.IgnoreCollision(col, hit.collider, true);
        Instantiate(waterSplash, hit.point + 2 * Vector3.up, Quaternion.identity, null);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Drowning");
        dummyTarget.parent = null;
        pM.canMove = false;

        if (heldItem != null)
        {
            heldItem.GrabRelease(true);
            Destroy(heldItem.gameObject);
        }

        pM._velocity = 0;
        float currentGravityMult = pM.gravityMultiplier;
        pM.gravityMultiplier = .03f;
        yield return new WaitForSeconds(drawningTimer);
        Physics.IgnoreCollision(col, hit.collider, false);
        pM._velocity = 0;
        _characterController.enabled = false;
        transform.position = respawnTile.transform.position + (25f + 3f) * Vector3.up;
        _characterController.enabled = true;
        pM.canMove = true;
/*        targetGroup.m_Targets[j].target = transform;
        sse.Screens[k].Target = transform ;*/
        dummyTarget.parent = transform;
        dummyTarget.localPosition = Vector3.zero;
        pM.gravityMultiplier = currentGravityMult;
        waterFlag = false;
        isDrawning = false;
    }

    private void OnDestroy()
    {
        movingSound.stop(STOP_MODE.IMMEDIATE);
        movingSound.release();
    }
}
