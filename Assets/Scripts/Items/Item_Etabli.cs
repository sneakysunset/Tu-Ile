using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
    bool isActive;
    Tile tileUnder;
    EtabliCanvas itemNum;
    bool isChantier;
    public UnityEvent EventOnCreation;
    [HideInInspector] public bool constructed = false;
    [HideInInspector] public bool isDestroyed = false;
    #endregion

    #region SystemCallbacks
    public override void Awake()
    {
        base.Awake();
        meshs = GetComponentsInChildren<MeshRenderer>();
        waiter = new WaitForSeconds(recette.convertionTime);
        rb.isKinematic = true;
        itemNum = GetComponentInChildren<EtabliCanvas>();
        stackT = transform.Find("Stacks");
    }

    private void Start()
    {
        recette.ResetRecette();
        if(Utils.IsSameOrSubclass(recette.craftedItemPrefab.GetType(), typeof(Item_Chantier))) isChantier = true;
        tileUnder = FindObjectOfType<TileSystem>().WorldPosToTile(transform.position);
        transform.parent = tileUnder.transform;
        createdItem = transform.Find("TileCreator/CreatedPos");
        itemNum.UpdateText(this);
    }

    public override void Update()
    {
        base.Update();

        if(isActive && !tileUnder.walkable)
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
    
    public override void GrabStarted(UnityEngine.Transform holdPoint, Player player)
    {
        //Transfer Items to Etabli
        TransferItems(player);

        //Update EtabliCanvas values
        itemNum.UpdateText(this);

        //Check if convertion can take place
        if (CheckStacks())
        {
            if(isChantier) StartCoroutine(ChantierConvert());
            else StartCoroutine(Convert());
        } 
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
                itemNum.gameObject.SetActive(true);
            }
            else
            {
                if(isChantier)
                {
                    Destroy(gameObject);
                }
                m.enabled = false;
                isActive = false;
                itemNum.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        if( isChantier && !constructed)
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
                    Highlight.SetActive(false);
                    break;
                }
            }
        }
    }

    IEnumerator Convert()
    {
        convertorFlag = true;
        yield return waiter;
        for (int i = 0; i < recette.requiredItemStacks.Length; i++)
        {
            recette.requiredItemStacks[i].currentNumber -= recette.requiredItemStacks[i].cost;
        }
        int f = 0;
        for (int i = 0; i < recette.requiredItemUnstackable.Length; i++)
        {
            recette.requiredItemUnstackable[i].isFilled = false;
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
        itemNum.UpdateText(this);
        convertorFlag = false;
        EventOnCreation?.Invoke();

    }

    IEnumerator ChantierConvert()
    {
        yield return waiter;
        GameObject chantier = Instantiate(recette.craftedItemPrefab, tileUnder.transform.position + Vector3.up * GameConstant.tileHeight, Quaternion.identity).gameObject;
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
            if(tileUnder != null)
            {
                etabli.transform.position = new Vector3(etabli.transform.position.x, tileUnder.transform.position.y + 23.4f, etabli.transform.position.z);
            }
        }
    }
}
#endif
#endregion