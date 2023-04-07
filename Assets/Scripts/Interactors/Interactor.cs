using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Interactor : MonoBehaviour
{
    public Tile.TileType type;
    public Mesh[] meshs;
    public Material[] materials;
    protected MeshRenderer meshR;
    protected MeshFilter meshF;
    public float regrowthTimer;
    protected float timer;
    protected int stateIndex;
    protected bool isInteractedWith;
    public bool interactable = true;
    protected float currentHitTimer;
    protected List<Player> _player;
    public GameObject spawnPrefab;
    public int ressourceNum = 10;
    Transform stackT;
    private Item_Stack stackItem;
    [Range(1, 10)] public int numberOfRessourceGenerated = 3;
    [HideNormalInspector] public bool fadeChecker;
    [HideNormalInspector] bool isFaded;
    private void Start()
    {
        stateIndex = meshs.Length - 1;
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        meshR.sharedMaterial = materials[stateIndex];
        meshF.mesh = meshs[stateIndex]; 
        _player = new List<Player>();
        Transform p = transform.parent.parent.parent;
        stackT = p.Find("StackPos");
        CreateStack();
    }

    void CreateStack()
    {
        if (stackT.childCount == 0)
        {
            GameObject obj = Instantiate(spawnPrefab, stackT.position, Quaternion.identity, null);
            obj.transform.parent = stackT;
            stackItem = obj.GetComponent<Item_Stack>();
            foreach(Player player in _player)
            {
                player.holdableItems.Add(stackItem);
            }
        }
        else
        {
            stackItem = stackT.GetChild(0).GetComponent<Item_Stack>();
        }
    }

    public virtual void OnInteractionEnter(float hitTimer, Player player)
    {
        if(_player.Count == 0) 
        { 
            
        }
        timer = hitTimer;
        currentHitTimer = hitTimer;
        _player.Add(player);
        isInteractedWith = true;
        if(stateIndex == meshs.Length - 1)
        {
            stateIndex--;
            meshF.mesh = meshs[stateIndex];
            meshR.material = materials[stateIndex];
        }
    }

    

    private void LateUpdate()
    {
        UnFadeTile();
    }

    private void Update()
    {
        isFaded = false;
        timer -= Time.deltaTime;
        if (isInteractedWith)
        {
            OnFilonMined();
        }
        else if(stateIndex < meshs.Length - 1)
        {
            if (timer <= 0)
            {
                timer = regrowthTimer;
                stateIndex = meshs.Length - 1;
            }
        }
        else if(!interactable && stateIndex == meshs.Length - 1)
        {
            interactable = true;
            meshF.mesh = meshs[stateIndex];
            meshR.material = materials[stateIndex];
        }
    }

    public virtual void OnInteractionExit()
    {
        OnEndInteraction();
    }

    protected virtual void OnEndInteraction()
    {
        isInteractedWith = false;
        timer = regrowthTimer;
        if(_player != null)
        {
            if(_player.Count > 0)
            {
                foreach (var player in _player) 
                { 
                    player.isMining = false;
                    player.interactor = null; 
                }
            }
            _player.Clear();
        }
    }

    protected virtual void EmptyInteractor()
    {
        CreateStack();
        stackItem.numberStacked += numberOfRessourceGenerated;
        //GameObject obj = Instantiate(spawnPrefab, spawnPoint.position, Quaternion.identity);
        //obj.transform.parent = this.transform;
        OnEndInteraction();
        interactable = false;
    }

    protected virtual void OnFilonMined()
    {
        if (timer <= 0 && stateIndex > 0)
        {
            switch (type)
            {
                case Tile.TileType.Wood:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Wood_Cutting");
                    break;
                case Tile.TileType.Rock:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                    break;
            }
            timer = currentHitTimer;
            stateIndex--;
            meshF.mesh = meshs[stateIndex];
            meshR.material = materials[stateIndex];
        }
        else if (stateIndex == 0)
        {
            EmptyInteractor();
        }
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
}
