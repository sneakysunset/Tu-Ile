using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EtabliCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Etabli etabli;
    public TextMeshProUGUI[] textsNear;
    public TextMeshProUGUI[] textsFar;
    Transform mainCamera;
    public UnityEngine.UI.Image[] imagesNear;
    public UnityEngine.UI.Image[] imagesFar;
    public UnityEngine.UI.Image resultImageFar;
    public UnityEngine.UI.Image resultImageNear;
    public RectTransform backGroundNear;
    public RectTransform backGroundFar;
    private RessourcesManager rMan;


    private void OnActivated()
    {
        rMan = FindObjectOfType<RessourcesManager>();
        cam = FindObjectOfType<CameraCtr>();
        backGroundFar.localScale = new Vector3(0.35f, backGroundFar.localScale.y, backGroundFar.localScale.z);
        backGroundNear.localScale = new Vector3(backGroundNear.localScale.x, 0.65f, backGroundNear.localScale.z);
        
        if(Camera.main) mainCamera = Camera.main.transform;
        for (int i = 0; i < imagesNear.Length; i++)
        {
            imagesNear[i].gameObject.SetActive(false);
            imagesFar[i].gameObject.SetActive(false);
            textsNear[i].gameObject.SetActive(false);
            if(i - 1 >= 0) textsFar[i - 1].gameObject.SetActive(false);
        }

        int f = 0;

        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    imagesNear[i].sprite = rMC.sprite;
                    imagesNear[i].gameObject.SetActive(true);
                    imagesFar[i].gameObject.SetActive(true);
                    imagesFar[i].sprite = rMC.sprite;
                    if(i - 1 >= 0) textsFar[i - 1].gameObject.SetActive(true);
                    textsNear[f].gameObject.SetActive(true);
                    backGroundNear.localScale += .35f * Vector3.up;
                    backGroundFar.localScale += .30f * Vector3.right;
                    RectTransform p = backGroundFar.parent as RectTransform;
                    //p.anchoredPosition += 75 * Vector2.right;
                    f++;
                }
            }

        }
        for (int i = 0; i < etabli.recette.requiredItemUnstackable.Length; i++)
        {
            foreach (ressourceMeshCollecUnstackable rMC in rMan.RessourceMeshsUnstackable)
            {
                if (rMC.itemType == etabli.recette.requiredItemUnstackable[i].itemType)
                {
                    imagesNear[f].gameObject.SetActive(true);
                    imagesNear[f].sprite = rMC.sprite;
                    imagesFar[f].gameObject.SetActive(true);
                    imagesFar[f].sprite = rMC.sprite;
                    if (f - 1 >= 0) textsFar[f - 1].gameObject.SetActive(true);
                    textsNear[f].gameObject.SetActive(true);
                    backGroundNear.localScale += .35f * Vector3.up;
                    backGroundFar.localScale += .35f * Vector3.right;
                    RectTransform p = backGroundFar.parent as RectTransform;
                    //p.anchoredPosition += 75 * Vector2.right;
                    f++;
                }
            }
        }

        foreach (var r in rMan.ressourceRecettesResults)
        {
            bool yo = false;
            switch (etabli.recette.craftedItemPrefab.GetType().ToString())
            {
                case "Item_Stack_Tile":
                Item_Stack_Tile iS = etabli.recette.craftedItemPrefab as Item_Stack_Tile;
                if (iS.stackType == r.tileType && r.isTile)
                {
                    resultImageFar.sprite = r.sprite;
                    resultImageNear.sprite = r.sprite;
                    yo = true;
                }
                break;
            case "Item_Bird":
                if (r.itemType == Item.ItemType.Bird && !r.isTile)
                {
                    resultImageFar.sprite = r.sprite;
                    resultImageNear.sprite = r.sprite;
                    yo = true;
                }
                break;
                case "Item_Boussole":
                if (r.itemType == Item.ItemType.Boussole && !r.isTile)
                {
                    resultImageFar.sprite = r.sprite;
                    resultImageNear.sprite = r.sprite;
                    yo = true;
                }
                break;
            }
            if (yo) break;
        }


    }



    public void UpdateText(Item_Etabli et)
    {
        int f = 0;
        if (etabli == null)
        {
            etabli = et;
            OnActivated();
        }
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    textsNear[i].text = "x " + etabli.recette.requiredItemStacks[i].currentNumber + " / " + etabli.recette.requiredItemStacks[i].cost.ToString();
                    if (etabli.recette.requiredItemStacks[i].currentNumber >= etabli.recette.requiredItemStacks[i].cost)
                    {
                        textsNear[i].color = Color.green;
                    }
                    else
                    {
                        textsNear[i].color = Color.white;
                    }
                    f++;
                    break;
                }
            }
        }

        for (int i = 0; i < etabli.recette.requiredItemUnstackable.Length; i++)
        {
            foreach (ressourceMeshCollecUnstackable rMC in rMan.RessourceMeshsUnstackable)
            {
                if (rMC.itemType == etabli.recette.requiredItemUnstackable[i].itemType)
                {
                    if (etabli.recette.requiredItemUnstackable[i].isFilled)
                    {
                        textsNear[f].text = " V";
                        textsNear[f].color = Color.green;
                    }
                    else
                    {
                        textsNear[f].text = "X";
                        textsNear[f].color = Color.white;
                    }
                    f++;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (mainCamera) transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }

    public void PlayerNear()
    {
        backGroundNear.parent.gameObject.SetActive(true);
        backGroundFar.parent.gameObject.SetActive(false);
    }

    public void PlayerFar()
    {
        backGroundFar.parent.gameObject.SetActive(true);
        backGroundNear.parent.gameObject.SetActive(false);
    }

    IEnumerator rebuildLayout()
    {
        print(90);
        GetComponent<Canvas>().worldCamera = Camera.main;
        GetComponentInChildren<HorizontalLayoutGroup>().enabled = false;
        yield return new WaitForEndOfFrame();
        GetComponentInChildren<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
        GetComponentInChildren<HorizontalLayoutGroup>().SetLayoutHorizontal();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();
        GetComponentInChildren<HorizontalLayoutGroup>().enabled = true;
    }
}
