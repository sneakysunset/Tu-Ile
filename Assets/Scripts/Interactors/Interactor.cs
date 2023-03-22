using UnityEngine;

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
    protected Interactions _player;
    public GameObject spawnPrefab;
    private Transform spawnPoint;
    public int ressourceNum = 10;
    private void Start()
    {
        stateIndex = meshs.Length;
        meshF = GetComponent<MeshFilter>();
        meshR = GetComponent<MeshRenderer>();
        spawnPoint = transform.Find("SpawnPoint");
    }

    public virtual void OnInteractionEnter(float hitTimer, Interactions player)
    {
        timer = hitTimer;
        currentHitTimer = hitTimer;
        _player = player;
        isInteractedWith = true;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (isInteractedWith)
        {
            if(timer <= 0 && stateIndex > 0)
            {
                timer = currentHitTimer;
                stateIndex--;
                meshF.mesh = meshs[stateIndex];
                meshR.material = materials[stateIndex];
                //GameObject obj = Instantiate(spawnPrefab, spawnPoint.position, Quaternion.identity);
                //obj.transform.parent = this.transform;
            }
            else if (timer <= 0 && stateIndex == 0)
            {
                EmptyInteractor();
            }
        }
        else if(stateIndex < meshs.Length - 1)
        {
            if (timer <= 0)
            {
                timer = regrowthTimer;
                stateIndex++;
                meshF.mesh = meshs[stateIndex];
                meshR.material = materials[stateIndex];
                interactable = true;
            }
        }
    }

    public virtual void OnInteractionExit()
    {
        OnEndInteraction();

    }

    protected virtual void EmptyInteractor()
    {
        _player.GetRessource(ressourceNum);
        OnEndInteraction();
        interactable = false;
    }

    protected virtual void OnEndInteraction()
    {
        isInteractedWith = false;
        timer = regrowthTimer;
        if(_player != null)
        {
            if(_player.interactor != null)
            {
            _player.interactor = null;
            }
            _player = null;
        }
    }
}
