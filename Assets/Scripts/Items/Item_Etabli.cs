using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

#region structs
[System.Serializable]
public struct stack
{
    public Item_Stack.StackType stackType;
    [HideInInspector] public int currentNumber;
    public int cost;
}

[System.Serializable]
public struct item
{
    public Item.ItemType itemType;
    [HideInInspector] public bool isFilled;
}
#endregion

public class Item_Etabli : Item
{
    #region variables
    public bool limitItemsReceived;
    public SO_Recette recette;
    Item craftedItem;
    private UnityEngine.Transform stackT;
    private bool convertorFlag;
    private WaitForSeconds waiter;
    UnityEngine.Transform createdItem;
    private MeshRenderer[] meshs;
    public FMOD.Studio.EventInstance creationInst;
    bool isActive;
    Tile tileUnder;
    EtabliCanvas itemNum;
    ChantierCanvas itemNumCh;
    public bool isChantier;
    public UnityEvent EventOnCreation;
    [HideInInspector] public bool constructed = false;
    [HideInInspector] public bool isDestroyed = false;
    [HideNormalInspector] public bool isNear;
    [HideNormalInspector] public List<Player> playersOn;
    public bool restraintEditorMovement = true;
    #endregion

    #region SystemCallbacks
    public override void Awake()
    {
        base.Awake();
        playersOn = new List<Player>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        waiter = new WaitForSeconds(recette.convertionTime);
        rb.isKinematic = true;

        stackT = transform.Find("Stacks");
        RessourcesManager rMan = FindObjectOfType<RessourcesManager>(); 
        for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
        {
            foreach(ressourceMeshCollecUnstackable r in rMan.RessourceMeshsUnstackable)
            {
                if (recette.requiredItemUnstackable[i].itemType == r.itemType)
                {
                    stackT.GetChild(i).GetComponent<MeshFilter>().mesh = r.mesh;
                    stackT.GetChild(i).GetComponent<MeshRenderer>().material = r.mat;
                    break;
                }
            }
        }
        for (int i = 0; i < recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec r in rMan.RessourceMeshs)
            {
                if (recette.requiredItemStacks[i].stackType == r.stackType)
                {
                    stackT.GetChild(i + recette.requiredItemUnstackable.Length).GetComponent<MeshFilter>().mesh = r.meshs[0];
                    stackT.GetChild(i + recette.requiredItemUnstackable.Length).GetComponent<MeshRenderer>().material = r.materials[0];
                    break;
                }
            }
        }
    }

    private void Start()
    {
        recette.ResetRecette();
        if(Utils.IsSameOrSubclass(recette.craftedItemPrefab.GetType(), typeof(Item_Chantier))) isChantier = true;
        tileUnder = TileSystem.Instance.WorldPosToTile(transform.position);
        transform.position = new Vector3(transform.position.x, tileUnder.transform.position.y + 23.4f, transform.position.z);
        transform.parent = tileUnder.transform;
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
    }

   
    public override void Update()
    {
            base.Update();

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
                if (CheckStacks())
                {
                    StartCoroutine(Convert());
                }
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
                    OnDestroyMethod();
                    Destroy(gameObject);
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
            FindObjectOfType<MissionManager>().CheckMissions();
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
                    if (!limitItemsReceived)
                    {
                        recette.requiredItemStacks[i].currentNumber += itemS.numberStacked;
                        player.heldItem = null;
                        Destroy(itemS.gameObject);
                    }
                    else
                    {
                        int numToTransfer = Mathf.Clamp(itemS.numberStacked, 0, iS.cost - iS.currentNumber);
                        recette.requiredItemStacks[i].currentNumber += numToTransfer;
                        itemS.numberStacked -= numToTransfer;
                        if(itemS.numberStacked <= 0)
                        {
                            recette.requiredItemStacks[i].currentNumber += itemS.numberStacked;
                            player.heldItem = null;
                            Destroy(itemS.gameObject);
                        }
                    }
                    if (recette.requiredItemStacks[i].currentNumber > 0)
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
                    recette.requiredItemUnstackable[i].isFilled = true;
                    Item tempItem = player.heldItem;
                    player.heldItem = null;
                    Destroy(tempItem.gameObject);
                    stackT.GetChild(i).gameObject.SetActive(true);

                    Highlight.SetActive(false);
                    break;
                }
            }
        }
    }

    IEnumerator Convert()
    {
        convertorFlag = true;
        FMODUtils.SetFMODEvent(ref creationInst, "event:/Tuile/Tile/TileCreation", transform);
        yield return waiter;
        FMODUtils.StopFMODEvent(ref creationInst, true);
        for (int i = 0; i < recette.requiredItemStacks.Length; i++)
        {
            recette.requiredItemStacks[i].currentNumber -= recette.requiredItemStacks[i].cost;
            if (recette.requiredItemStacks[i].currentNumber == 0)
            {
                stackT.GetChild(i + recette.requiredItemUnstackable.Length).gameObject.SetActive(false);
            }
        }
        int f = 0;
        for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
        {
            recette.requiredItemUnstackable[i].isFilled = false;
            stackT.GetChild(i).gameObject.SetActive(false);
            f++;
        }
        if(craftedItem == null)
        {
            craftedItem = Instantiate(recette.craftedItemPrefab, createdItem.position, Quaternion.identity, createdItem);
        }
        if(Utils.IsSameOrSubclass(System.Type.GetType("Item_Stack"), craftedItem.GetType()))
        {
            Item_Stack craIt = craftedItem as Item_Stack;
            craIt.numberStacked += recette.numberOfCrafted;
        }
        if(isChantier) itemNumCh.UpdateText(this);
        else itemNum.UpdateText(this);
        convertorFlag = false;
        EventOnCreation?.Invoke();

    }

    IEnumerator ChantierConvert()
    {
        yield return waiter;
        GameObject chantier = Instantiate(recette.craftedItemPrefab, tileUnder.transform.position + recette.craftedItemPrefab.transform.position + Vector3.up * GameConstant.tileHeight, recette.craftedItemPrefab.transform.rotation).gameObject;
        chantier.transform.parent = tileUnder.transform;
        constructed = true;
        FindObjectOfType<MissionManager>().CheckMissions();
        EventOnCreation?.Invoke();
        gameObject.SetActive(false);
    }

    bool CheckStacks()
    {
        int f = 0;
        foreach (var i in recette.requiredItemStacks)
        {
            if (i.cost <= i.currentNumber)
            {
                f++;
            }
        }
        foreach(var i in recette.requiredItemUnstackable)
        {
            if(i.isFilled)
            {
                f++;
            }
        }
        bool condition1 = f == recette.requiredItemStacks.Length + recette.requiredItemUnstackable.Length;
        bool condition2 = recette.craftedItemPrefab.GetType() == System.Type.GetType("Item_Stack");
        bool condition3 = craftedItem == null;

        if (!convertorFlag && condition1 && (condition2 || condition3))
        {
            return true;
        }
        else return false;
    }


    void OnValidate() { UnityEditor.EditorApplication.delayCall += _OnValidate; }
    private void _OnValidate()
    {
        if (!Application.isPlaying)
        {
            itemNum = GetComponentInChildren<EtabliCanvas>();
            itemNum.OnActivated();
        }
    }
    #endregion
}

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(Item_Etabli))]
[System.Serializable]
public class EtablieSystemEditor : Editor
{
    Item_Etabli etabli;

    private void OnEnable()
    {
        etabli = (Item_Etabli)target;
    }


    private void OnSceneGUI()
    {
        if (!Application.isPlaying)
        {
            Draw();
            EditorUtility.SetDirty(etabli);
        }
    }

    void Draw()
    {
        if (!Application.isPlaying)
        {
            Tile tileUnder = TileSystem.Instance.WorldPosToTile(etabli.transform.position);
            if(tileUnder != null && etabli.restraintEditorMovement)
            {
                float yAngle = etabli.transform.eulerAngles.y - (etabli.transform.eulerAngles.y - 30) % 60;
                Quaternion quat = Quaternion.Euler(0, yAngle, 0);
                etabli.transform.rotation = quat;
                etabli.transform.position = new Vector3(etabli.transform.position.x, tileUnder.transform.position.y + GameConstant.tileHeight, etabli.transform.position.z);
            }
        }
    }
}
#endif
#endregion