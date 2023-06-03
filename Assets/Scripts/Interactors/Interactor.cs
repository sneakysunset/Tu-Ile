using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public struct visualComp
{
    public Mesh mesh;
    public Material material;
}

public class Interactor : MonoBehaviour
{
    #region Variables

    #region mainVariables
    public TileType type;
    [HideInInspector] public List<Player> _player;
    protected int stateIndex;
    public float hitRateSpeedModifier;
    #endregion

    #region Regrowth
    IEnumerator regrowth;
    WaitForSeconds regrowthWaiter;
    public float regrowthTimer;
    #endregion

    #region Mesh Variables
    public visualComp[] comp;
    protected MeshRenderer meshR;
    protected MeshFilter meshF;
    protected MeshCollider meshC;
    public Transform target;
    #endregion

    #region States
    [HideNormalInspector] public bool isInteractedWith;
    [HideNormalInspector] public bool interactable = true;
    #endregion

    #region Ressource Stacks
    public Item_Stack stackPrefab;
    Transform stackPosT;
    [HideInInspector] public bool isTuto;
    [HideInInspector] public Item_Stack stackItem;
    #endregion

    #region Fade
    [HideNormalInspector] public bool fadeChecker;
    [HideNormalInspector] bool isFaded;
    [Range(1, 10)] public int numberOfRessourceGenerated = 3;
    #endregion

    #region Feedbacks
    bool ps;
    [HideInInspector] public ParticleSystem pSysRegrowth;
    #endregion
    #endregion
    public float scalerDiff;
    #region SystemCallBacks
    private void Start()
    {
        /*        regrowthTimer /= comp.Length;
                pSysRegrowth = GetComponentInChildren<ParticleSystem>();
                regrowthWaiter = new WaitForSeconds(regrowthTimer);
                stateIndex = comp.Length - 1;
                meshR.sharedMaterial = comp[stateIndex].material;
                meshF.mesh = comp[stateIndex].mesh; 
                meshC.sharedMesh = comp[stateIndex].mesh; 
                _player = new List<Player>();
                Transform p = transform.parent.parent.parent;
                stackPosT = p.Find("StackPos");
                CreateStack();*/
        transform.localScale *= UnityEngine.Random.Range(1 - scalerDiff, 1 +scalerDiff);
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        meshC = GetComponent<MeshCollider>();
    }

    private void OnEnable()
    {
        regrowthTimer /= comp.Length;
        pSysRegrowth = GetComponentInChildren<ParticleSystem>();
        regrowthWaiter = new WaitForSeconds(regrowthTimer);
        stateIndex = comp.Length - 1;
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        meshC = GetComponent<MeshCollider>();
        meshR.sharedMaterial = comp[stateIndex].material;
        meshF.mesh = comp[stateIndex].mesh;
        meshC.sharedMesh = comp[stateIndex].mesh;
        _player = new List<Player>();
        if(transform.parent == null || transform.parent.parent || transform.parent.parent.parent == null)
        {
            return;
        }
        Transform p = transform.parent.parent.parent;
        stackPosT = p.Find("StackPos");
        Debug.Log(TileSystem.Instance);
        if(!TileSystem.Instance.isHub)CreateStack();
    }

    private void Update()
    {
        //isFaded = false;
    }

    public virtual void OnInteractionEnter(Player player)
    {
        player.pState = Player.PlayerState.Mine;
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
            player.pState = Player.PlayerState.Idle;
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
        if(stackPosT == null)
        {
            Transform p = transform.parent.parent.parent;
            stackPosT = p.Find("StackPos");
        }
        if (stackPosT.childCount == 0)  
        {
            Transform tr = stackPosT;
            Vector3 pos = stackPosT.position;
            Item_Stack obj = ObjectPooling.SharedInstance.GetPoolItem(5, pos, tr).GetComponent<Item_Stack>();

            //Item_Stack obj = Instantiate(stackPrefab, stackPosT.position, Quaternion.identity, null);
            obj.numberStacked = 0;
            switch (type)
            {
                case TileType.Wood: obj.stackType = Item.StackType.Wood; break;
                case TileType.Rock: obj.stackType = Item.StackType.Rock; break;
                case TileType.Gold: obj.stackType = Item.StackType.Gold; break;
                case TileType.Diamond: obj.stackType = Item.StackType.Diamond; break;
                case TileType.Adamantium: obj.stackType = Item.StackType.Adamantium; break;
            }
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
            meshF.mesh = comp[stateIndex].mesh;
            meshC.sharedMesh = comp[stateIndex].mesh;
            meshR.material = comp[stateIndex].material;
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
        if (TileSystem.Instance.isHub && isTuto)
        {
            TutorialManager tuto = TileSystem.Instance.tutorial;
            isTuto = false;
            if (tuto.enumer != null) tuto.StopCoroutine(tuto.enumer);
            tuto.enumer = null;
            if (tuto.tileSpawned) tuto.enumer = tuto.GetChantier();
            else tuto.enumer = tuto.GetEtabli();
            tuto.StartCoroutine(tuto.enumer);
        }
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
        while (stateIndex < comp.Length - 1)
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
        meshF.mesh = comp[stateIndex].mesh;
        meshC.sharedMesh = comp[stateIndex].mesh;
        meshR.material = comp[stateIndex].material;
        interactable = true;
        tag = "Interactor";
        regrowth = null;
    }
    #endregion

    #region FadeTile
    private void LateUpdate()
    {
        //UnFadeTile();
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
