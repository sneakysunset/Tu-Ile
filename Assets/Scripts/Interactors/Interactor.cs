using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


public class Interactor : MonoBehaviour
{
    #region Variables

    #region mainVariables
    public Tile.TileType type;
    [HideInInspector] public List<Player> _player;
    protected int stateIndex;
    #endregion

    #region Regrowth
    IEnumerator regrowth;
    WaitForSeconds regrowthWaiter;
    public float regrowthTimer;
    #endregion

    #region Mesh Variables
    public Mesh[] meshs;
    public Material[] materials;
    protected MeshRenderer meshR;
    protected MeshFilter meshF;
    protected MeshCollider meshC;
    #endregion

    #region States
    public bool isInteractedWith;
    public bool interactable = true;
    #endregion

    #region Ressource Stacks
    public Item_Stack stackPrefab;
    Transform stackPosT;
    private Item_Stack stackItem;
    #endregion

    #region Fade
    [HideNormalInspector] public bool fadeChecker;
    [HideNormalInspector] bool isFaded;
    [Range(1, 10)] public int numberOfRessourceGenerated = 3;
    #endregion

    #region Feedbacks
    bool ps;
    public ParticleSystem pSysRegrowth;
    #endregion
    #endregion

    #region SystemCallBacks
    private void Start()
    {
        regrowthTimer /= meshs.Length;
        regrowthWaiter = new WaitForSeconds(regrowthTimer);
        stateIndex = meshs.Length - 1;
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        meshC = GetComponent<MeshCollider>();
        meshR.sharedMaterial = materials[stateIndex];
        meshF.mesh = meshs[stateIndex]; 
        meshC.sharedMesh = meshs[stateIndex]; 
        _player = new List<Player>();
        Transform p = transform.parent.parent.parent;
        stackPosT = p.Find("StackPos");
        CreateStack();
    }

    private void Update()
    {
        isFaded = false;
    }

    public virtual void OnInteractionEnter(Player player)
    {
        player.isMining = true;
        if(!_player.Contains(player))
        {
            _player.Add(player);
        }
        isInteractedWith = true;
        if(regrowth != null) StopCoroutine(regrowth);
    }

    public virtual void OnInteractionExit(Player player)
    {
        if (_player.Contains(player)) _player.Remove(player);
        if (player.interactors.Contains(this)) player.interactors.Remove(this);
        if(player.interactors.Count == 0)
        {
            player.isMining = false;
        }
        if (_player.Count == 0)
        {
            isInteractedWith = false;
        }
    }

    #endregion

    #region Other Methods
    void CreateStack()
    {
        if (stackPosT.childCount == 0)
        {
            Item_Stack obj = Instantiate(stackPrefab, stackPosT.position, Quaternion.identity, null);
            obj.transform.parent = stackPosT;
            stackItem = obj;
            foreach(Player player in _player)
            {
                player.holdableItems.Add(stackItem);
            }
        }
        else
        {
            stackItem = stackPosT.GetChild(0).GetComponent<Item_Stack>();
        }
    }

    public virtual void OnFilonMined()
    {
        if (stateIndex > 0)
        {
            stateIndex--;
            meshF.mesh = meshs[stateIndex];
            meshC.sharedMesh = meshs[stateIndex];
            meshR.material = materials[stateIndex];
        }

        if (stateIndex == 0)
        {
            EmptyInteractor();
        }
    }
    
    protected virtual void EmptyInteractor()
    {
        CreateStack();
        stackItem.numberStacked += numberOfRessourceGenerated;

        for (int i = _player.Count - 1; i <= 0 ; i--)
        {
            if (i < 0) break;
            OnInteractionExit(_player[i]);
        }
        regrowth = Regrowth();
        StartCoroutine(regrowth);
        interactable = false;
        tag = "InteractorOff";
    }

    IEnumerator Regrowth()
    {
        while (stateIndex < meshs.Length - 1)
        {
            stateIndex++;
/*            if(interactable)
            {
                meshF.mesh = meshs[stateIndex];
                meshC.sharedMesh = meshs[stateIndex];
                meshR.material = materials[stateIndex];
            }*/
            yield return regrowthWaiter;
        }
        meshF.mesh = meshs[stateIndex];
        meshC.sharedMesh = meshs[stateIndex];
        meshR.material = materials[stateIndex];
        interactable = true;
        tag = "Interactor";
        regrowth = null;
    }
    #endregion

    #region FadeTile
    private void LateUpdate()
    {
        UnFadeTile();
    }

    public void FadeTile(float t)
    {
        isFaded = true;
        if (!fadeChecker)
        {
            fadeChecker = true;
            ChangeRenderMode.ChangeRenderModer(meshR.material, ChangeRenderMode.BlendMode.Transparent);
            Color col = meshR.material.color;
            col.a = t;
            meshR.material.color = col;
        }
    }

    private void UnFadeTile()
    {
        if (!isFaded && fadeChecker)
        {
            fadeChecker = false;
            ChangeRenderMode.ChangeRenderModer(meshR.material, ChangeRenderMode.BlendMode.Opaque);
            Color col = meshR.material.color;
            col.a = .2f;
            meshR.material.color = col;
        }
    }
    #endregion
}
