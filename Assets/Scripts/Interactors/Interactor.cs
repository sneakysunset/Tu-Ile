using UnityEngine;
using System;
using System.Collections.Generic;
public class Interactor : MonoBehaviour
{
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
    protected List<Interactions> _player;
    public GameObject spawnPrefab;
    private Transform spawnPoint;
    public int ressourceNum = 10;

    private void Start()
    {
        stateIndex = meshs.Length - 1;
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        meshR.sharedMaterial = materials[stateIndex];
        meshF.mesh = meshs[stateIndex]; 
        spawnPoint = transform.Find("SpawnPoint");
        _player = new List<Interactions>();
    }

    public virtual void OnInteractionEnter(float hitTimer, Interactions player)
    {
        timer = hitTimer;
        currentHitTimer = hitTimer;
        _player.Add(player);
        isInteractedWith = true;
    }

    private void Update()
    {
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
        GameObject obj = Instantiate(spawnPrefab, spawnPoint.position, Quaternion.identity);
        obj.transform.parent = this.transform;
        OnEndInteraction();
        interactable = false;
    }

    protected virtual void OnFilonMined()
    {
        if (timer <= 0 && stateIndex > 0)
        {
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
}
