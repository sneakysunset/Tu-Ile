using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#region structs
[System.Serializable]
public struct stack
{
    public Item_Stack.StackType stackType;
    public int cost;
}

[System.Serializable]
public struct item
{
    public Item.ItemType itemType;
}
#endregion

public class Item_Etabli : Item
{
    #region variables
    public SO_Recette recette;
    [HideInInspector] public Item craftedItem;
    [HideInInspector] public UnityEngine.Transform stackT;
    private bool convertorFlag;
    private WaitForSeconds waiter;
    UnityEngine.Transform createdItem;
    private MeshRenderer[] meshs;
    public FMOD.Studio.EventInstance creationInst;
    bool isActive;
    Tile tileUnder;
    EtabliCanvas itemNum;
    ChantierCanvas itemNumCh;
    [HideNormalInspector] public bool isChantier;
    [HideInInspector] public bool constructed = false;
    [HideInInspector] public bool isDestroyed = false;
    [HideNormalInspector] public bool isNear;
    [HideInInspector] public List<Player> playersOn;
    public bool restraintEditorMovement = true;
    Transform spawnPos;
    [HideNormalInspector] public int[] currentStackRessources;
    [HideNormalInspector] public bool[] itemsFilled;
    [ShowIf("isChantier")] public Material[] houseMaterials;
    [ShowIf("isChantier")] public Mesh houseMesh;
    public Item_Crate crate;
    #endregion

    #region SystemCallbacks
    public override void Awake()
    {
        base.Awake();
        if(isChantier) spawnPos = transform.GetChild(0);
        else spawnPos = transform.GetChild(1);
        playersOn = new List<Player>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        waiter = new WaitForSeconds(recette.convertionTime);
        rb.isKinematic = true;

        stackT = transform.Find("Stacks");
    }

    private void OnEnable()
    {
        itemsFilled = new bool[recette.requiredItemUnstackable.Length];
        currentStackRessources = new int[recette.requiredItemStacks.Length];
        if(Utils.IsSameOrSubclass(recette.craftedItemPrefab.GetType(), typeof(Item_Chantier))) isChantier = true;
        tileUnder = GridUtils.WorldPosToTile(transform.position);

        tileUnder.etabli = this;
        createdItem = transform.Find("TileCreator/CreatedPos");
        if (isChantier)
        {
            itemNumCh = GetComponentInChildren<ChantierCanvas>();
            itemNumCh.UpdateText(this);
        }
        else
        {
            itemNum = GetComponentInChildren<EtabliCanvas>();
            itemNum.UpdateText(this);
        }
        //yield return new WaitForSeconds(.01f);
        transform.parent = tileUnder.transform;
        int yoffset = 2;
        if (isChantier) yoffset = 0;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Tile")))
        {
            transform.position = hit.point + transform.localScale.y * yoffset * Vector3.up;
            transform.LookAt(new Vector3(tileUnder.transform.position.x, transform.position.y, tileUnder.transform.position.z));
            spawnPos.position = new Vector3(tileUnder.minableItems.position.x, transform.position.y, tileUnder.minableItems.position.z); 
        }
    }

    private void OnDisable()
    {
        if(FMODUtils.IsPlaying(creationInst)) FMODUtils.StopFMODEvent(ref creationInst, true);
    }

    public virtual IEnumerator OnPooled(SO_Recette _recette)
    {

        tileUnder = GridUtils.WorldPosToTile(transform.position);
        tileUnder.etabli = this;
        transform.parent = tileUnder.transform;
        if (Utils.IsSameOrSubclass(recette.craftedItemPrefab.GetType(), typeof(Item_Chantier))) isChantier = true;
        int yoffset = 2;
        if (isChantier) yoffset = 0;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Tile")))
        {
            transform.position = hit.point + transform.localScale.y * yoffset * Vector3.up;
            transform.LookAt(new Vector3(tileUnder.transform.position.x, transform.position.y, tileUnder.transform.position.z));
        }

        //recette = _recette;
        yield return new WaitForEndOfFrame();
        if (isChantier)
        {
            itemNumCh = GetComponentInChildren<ChantierCanvas>();
            itemNumCh.UpdateText(this);
        }
        else
        {
            itemNum = GetComponentInChildren<EtabliCanvas>();
            itemNum.UpdateText(this);
        }
    }

    public override void Update()
    {
        base.Update();

        if (Time.timeScale == 0 && FMODUtils.IsPlaying(creationInst)) creationInst.setPaused(true);
        else if (Time.timeScale == 1 && FMODUtils.IsPlaying(creationInst)) creationInst.setPaused(false);

        if (isActive && !tileUnder.walkable)
        {
            SetActiveMesh(false);
        }
        else if(!isActive && tileUnder.walkable)
        {
            SetActiveMesh(true);
        }
        
        if(craftedItem != null && craftedItem.isHeld)
        {
            craftedItem = null;
        }
    }
    
    public void PlayerNear()
    {
        if(!isChantier) itemNum.PlayerNear();
    }

    public void PlayerFar()
    {
        if(!isChantier) itemNum.PlayerFar();
    }

    public override void GrabStarted(UnityEngine.Transform holdPoint, Player player)
    {
        //Transfer Items to Etabli
        TransferItems(player);

        //Update EtabliCanvas values
        if (isChantier) itemNumCh.UpdateText(this);
        else itemNum.UpdateText(this);

        //Check if convertion can take place
        if (CheckStacks())
        {
            if(isChantier) StartCoroutine(ChantierConvert());
            else StartCoroutine(Convert());
        }
        
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Hit");
    }
    #endregion

    #region Methods
    public void SetActiveMesh(bool active)
    {
        foreach(MeshRenderer m in meshs)
        {
            if (active)
            {
                m.enabled = true;
                isActive = true;
                if(isChantier) itemNumCh.gameObject.SetActive(true);
                else itemNum.gameObject.SetActive(true);
            }
            else
            {
                if(isChantier)
                {
                    //OnDestroyMethod();
                    //Destroy(gameObject);
                }
                m.enabled = false;
                isActive = false;
                if(isChantier) itemNumCh.gameObject.SetActive(false);
                else itemNum.gameObject.SetActive(false);
            }
        }
    }

    public void OnDestroyMethod()
    {
        isDestroyed = true;
        if (isChantier && tileUnder.tileSpawnType != TileType.construction) tileUnder.tileSpawnType = TileType.construction;
        if (isChantier && !constructed)
        {
            //FindObjectOfType<MissionManager>().CheckMissions();
        }
    }



    void TransferItems(Player player)
    {
        // if ItemStacked check if item receivable and transfer to etabli
        if (player.heldItem && player.heldItem.GetType() == typeof(Item_Stack))
        {
            Item_Stack itemS = player.heldItem as Item_Stack;
            for (int i = 0; i < recette.requiredItemStacks.Length; i++)
            {
                stack iS = recette.requiredItemStacks[i];

                if (iS.stackType == itemS.stackType)
                {
                    if (!isChantier)
                    {
                        currentStackRessources[i] += itemS.numberStacked;
                        player.heldItem = null;
                        ObjectPooling.SharedInstance.RemovePoolItem(0, itemS.gameObject, itemS.GetType().ToString());
                        //Destroy(itemS.gameObject);
                    }
                    else
                    {
                        int numToTransfer = Mathf.Clamp(itemS.numberStacked, 0, iS.cost - currentStackRessources[i]);
                        currentStackRessources[i] += numToTransfer;
                        itemS.numberStacked -= numToTransfer;
                        if(itemS.numberStacked <= 0)
                        {
                            currentStackRessources[i] += itemS.numberStacked;
                            player.heldItem = null;
                            //Destroy(itemS.gameObject);
                            ObjectPooling.SharedInstance.RemovePoolItem(0, itemS.gameObject, itemS.GetType().ToString());
                        }
                    }
                    if (currentStackRessources[i] > 0)
                    {
                        stackT.GetChild(i + recette.requiredItemUnstackable.Length).gameObject.SetActive(true);
                    }
                    else
                    {
                        stackT.GetChild(i + recette.requiredItemUnstackable.Length).gameObject.SetActive(false);
                    }
                    Highlight.SetActive(false);
                    break;
                }
            }
        }
        //same but for not stackable Items
        else if (player.heldItem)
        {
            for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
            {
                item iT = recette.requiredItemUnstackable[i];
                if (Utils.GetTypeItem(iT.itemType, player.heldItem.GetType(), out System.Type type))
                {
                    itemsFilled[i] = true;
                    Item tempItem = player.heldItem;
                    player.heldItem = null;
                    //Destroy(tempItem.gameObject);

                    ObjectPooling.SharedInstance.RemovePoolItem(0, tempItem.gameObject, tempItem.GetType().ToString());
                    stackT.GetChild(i).gameObject.SetActive(true);

                    Highlight.SetActive(false);
                    break;
                }
            }
        }
    }

    IEnumerator Convert()
    {
        if (craftedItem != null && !Utils.IsSameOrSubclass(typeof(Item_Stack), craftedItem.GetType())) yield break ;

        do
        {
            convertorFlag = true;
            FMODUtils.SetFMODEvent(ref creationInst, "event:/Tuile/Tile/TileCreation", transform);
            yield return waiter;
            FMODUtils.StopFMODEvent(ref creationInst, true);
            for (int i = 0; i < recette.requiredItemStacks.Length; i++)
            {
                currentStackRessources[i] -= recette.requiredItemStacks[i].cost;
                if (currentStackRessources[i] == 0)
                {
                    stackT.GetChild(i + recette.requiredItemUnstackable.Length).gameObject.SetActive(false);
                }
            }
            int f = 0;
            for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
            {
                itemsFilled[i] = false;
                stackT.GetChild(i).gameObject.SetActive(false);
                f++;
            }
            if (craftedItem == null)
            {
                //craftedItem = Instantiate(recette.craftedItemPrefab, createdItem.position, Quaternion.identity, createdItem);
                Vector3 pos = createdItem.position;
                Transform tr = createdItem;
                craftedItem = ObjectPooling.SharedInstance.GetPoolItem(0, pos, tr, recette.craftedItemPrefab.GetType().ToString()).GetComponent<Item>();
            }

            if (Utils.IsSameOrSubclass(System.Type.GetType("Item_Stack"), craftedItem.GetType()))
            {
                Item_Stack craIt = craftedItem as Item_Stack;
                craIt.stackType = recette.craftedItemPrefab.GetComponent<Item_Stack>().stackType;
                craIt.numberStacked += recette.numberOfCrafted;
            }
            if (isChantier) itemNumCh.UpdateText(this);
            else itemNum.UpdateText(this);
        convertorFlag = false;
        }
        while (CheckStacks());

        if(TileSystem.Instance.isHub && isTuto)
        {
            isTuto = false;
            TutorialManager tuto = TileSystem.Instance.tutorial;
            if (tuto.enumer != null) StopCoroutine(tuto.enumer);
            tuto.enumer = tuto.GetSunkTile();
            StartCoroutine(tuto.enumer);
        }
        
    }

    IEnumerator ChantierConvert()
    {
        FMODUtils.SetFMODEvent(ref creationInst, "event:/Tuile/Tile/TileCreation", transform);
        yield return waiter;
        FMODUtils.StopFMODEvent(ref creationInst, true);
        //GameObject chantier = Instantiate(recette.craftedItemPrefab, tileUnder.transform.position + recette.craftedItemPrefab.transform.position + Vector3.up * GameConstant.tileHeight, recette.craftedItemPrefab.transform.rotation).gameObject;
        //chantier.transform.parent = tileUnder.transform;
        constructed = true;
        //FindObjectOfType<MissionManager>().CheckMissions();
        SO_Recette_Chantier re = recette as SO_Recette_Chantier;
        /*Transform house = */craftedItem = Instantiate(crate, transform.parent.GetChild(0).GetChild(4).position + .5f * Vector3.up, Quaternion.identity)/*.transform*/;
        Item_Crate it = craftedItem as Item_Crate;
        it.reward = re.reward;
        itemNumCh.gameObject.SetActive(false);
        //float targetY = house.position.y;
        //house.position -= 5f * Vector3.up;
        //house.DOMoveY(targetY, 2).SetEase(TileSystem.Instance.easeOut);
        GetComponentInChildren<MeshRenderer>().materials = houseMaterials;
        GetComponentInChildren<MeshFilter>().mesh = houseMesh;
        Highlight.SetActive(false);
        this.enabled = false; 
        
    }

    bool CheckStacks()
    {
        int f = 0;
        for (int i = 0; i < recette.requiredItemStacks.Length; i++)
        {
            if (recette.requiredItemStacks[i].cost <= currentStackRessources[i]) f++;
        }
        for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
        {
            if (itemsFilled[i]) f++;
        }
        bool condition1 = f == recette.requiredItemStacks.Length + recette.requiredItemUnstackable.Length;
        bool condition2 = Utils.IsSameOrSubclass(typeof(Item_Stack), recette.craftedItemPrefab.GetType());
        bool condition3 = craftedItem == null;

        if (!convertorFlag && condition1 && (condition2 || condition3))
        {
            return true;
        }
        else return false;
    }

#if UNITY_EDITOR
    void OnValidate() { UnityEditor.EditorApplication.delayCall += _OnValidate; }
    private void _OnValidate()
    {
        if (!Application.isPlaying && !isChantier)
        {
            itemNum = GetComponentInChildren<EtabliCanvas>();
            itemNum.OnActivated();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Tile tileUnder = GridUtils.WorldPosToTile(transform.position);
            if (tileUnder != null && restraintEditorMovement && (transform.rotation.eulerAngles.y % 60 != 0 || transform.position.y != tileUnder.transform.position.y + GameConstant.tileHeight + .4f))
            {
                float yAngle = transform.eulerAngles.y - (transform.eulerAngles.y - 30) % 60;
                Quaternion quat = Quaternion.Euler(0, yAngle, 0);
                transform.rotation = quat;
                transform.position = new Vector3(transform.position.x, tileUnder.transform.position.y + GameConstant.tileHeight + .4f, transform.position.z);
            }
        }
    }
#endif
#endregion
}