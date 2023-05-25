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
    [HideNormalInspector] public Item_Etabli etabli;
    public TextMeshProUGUI[] textsNear;
    public TextMeshProUGUI[] textsFar;
    [HideNormalInspector] public Transform mainCamera;
    public UnityEngine.UI.Image[] imagesNear;
    public UnityEngine.UI.Image[] imagesFar;
    public UnityEngine.UI.Image resultImageFar;
    public UnityEngine.UI.Image resultImageNear;
    public TextMeshProUGUI resultTextNear;
    public RectTransform backGroundNear;
    public RectTransform backGroundFar;
    private RessourcesManager rMan;


    public void OnActivated()
    {
        if (!Application.isPlaying) etabli = GetComponentInParent<Item_Etabli>();
        rMan = FindObjectOfType<RessourcesManager>();
        cam = FindObjectOfType<CameraCtr>();
        backGroundFar.sizeDelta = new Vector2(250, backGroundFar.sizeDelta.y);
        backGroundNear.sizeDelta = new Vector2(backGroundNear.sizeDelta.x, 180);
        RectTransform pi = imagesFar[0].rectTransform.parent as RectTransform;
        pi.anchoredPosition = new Vector2(-22.5f, pi.anchoredPosition.y);
        for (int i = 0; i < imagesNear.Length; i++)
        {
            imagesNear[i].transform.parent.gameObject.SetActive(false);
            //textsNear[i].gameObject.SetActive(false);
            imagesFar[i].gameObject.SetActive(false);
            if (i - 1 >= 0) textsFar[i - 1].gameObject.SetActive(false);
        }

        int f = 0;
        if (!etabli || !etabli.recette) return;
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    imagesNear[i].sprite = rMC.sprite;
                    imagesNear[i].transform.parent.gameObject.SetActive(true);
                    imagesFar[i].gameObject.SetActive(true);
                    //textsNear[f].gameObject.SetActive(true);
                    imagesFar[i].sprite = rMC.sprite;
                    if (i - 1 >= 0) textsFar[i - 1].gameObject.SetActive(true);
                    if(f > 0)
                    {
                        backGroundNear.sizeDelta += 45 * Vector2.up;
                        backGroundFar.sizeDelta += 90 * Vector2.right;
                    }

                    RectTransform p = imagesFar[0].rectTransform.parent as RectTransform;
                    if (f == 0)
                    {
                        p.anchoredPosition3D += 22 * Vector3.right;
                    }
                    else
                    {
                        p.anchoredPosition3D += -49 * Vector3.right;
                    }
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
                    imagesNear[f].transform.parent.gameObject.SetActive(true);
                    //imagesNear[f].sprite = rMC.sprite;
                    imagesFar[f].gameObject.SetActive(true);
                    imagesFar[f].sprite = rMC.sprite;
                    if (f - 1 >= 0) textsFar[f - 1].gameObject.SetActive(true);
                    textsNear[f].gameObject.SetActive(true);
                    if(f > 0)
                    {
                        backGroundNear.sizeDelta += 45 * Vector2.up;
                        backGroundFar.sizeDelta += 90 * Vector2.right;
                    }

                    RectTransform p = imagesFar[0].rectTransform.parent as RectTransform;
                    if (f == 0) p.anchoredPosition3D += 22 * Vector3.right;
                    else p.anchoredPosition3D += -49 * Vector3.right;
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
                case "Item_Bait":
                    if(r.itemType == Item.ItemType.Bait && !r.isTile)
                    {
                        resultImageFar.sprite = r.sprite;
                        resultImageNear.sprite = r.sprite;
                    }
                    break;
                case "Item_Crate":
                    if (r.itemType == Item.ItemType.Crate && !r.isTile)
                    {
                        resultImageFar.sprite = r.sprite;
                        resultImageNear.sprite = r.sprite;
                    }
                    break;
            }
            if (yo) break;
        }

        resultTextNear.text = "x " + etabli.recette.numberOfCrafted;
    }


    public void UpdateText(Item_Etabli et)
    {
        int f = 0;
        if (etabli == null || rMan == null || mainCamera == null)
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
                    textsNear[i].text = "x " + etabli.currentStackRessources[i] + " / " + etabli.recette.requiredItemStacks[i].cost.ToString();
                    if (etabli.currentStackRessources[i] >= etabli.recette.requiredItemStacks[i].cost)
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
                    if (etabli.itemsFilled[i])
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
        if(mainCamera == null && Camera.main) mainCamera = Camera.main.transform;
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

}
