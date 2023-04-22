using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class EtabliCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Etabli etabli;
    TextMeshProUGUI[] texts;
    Transform mainCamera;
    Image[] images;
    private RessourcesManager rMan;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        texts = GetComponentsInChildren<TextMeshProUGUI>();
        etabli = transform.parent.GetComponent<Item_Etabli>();
        mainCamera = Camera.main.transform;
        images = GetComponentsInChildren<Image>();
        rMan = FindObjectOfType<RessourcesManager>();

        for (int i = 0; i < images.Length - 1; i++)
        {
            images[i + 1].gameObject.SetActive(false);
            texts[i].text = string.Empty;
        }

        int f = 0;

        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    images[i + 1].sprite = rMC.sprite;
                    images[i + 1].gameObject.SetActive(true);
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
                    images[f + 1].gameObject.SetActive(true);
                    images[f + 1].sprite = rMC.sprite;
                    f++;
                }
            }
        }
    }

    public void UpdateText()
    {
        int f = 0;
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    texts[i].text = "x " + etabli.recette.requiredItemStacks[i].currentNumber + " / " + etabli.recette.requiredItemStacks[i].cost.ToString();
                    if (etabli.recette.requiredItemStacks[i].currentNumber >= etabli.recette.requiredItemStacks[i].cost)
                    {
                        texts[i].color = Color.green;
                    }
                    else
                    {
                        texts[i].color = Color.white;
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
                        texts[f].text = " V";
                        texts[f].color = Color.green;
                    }
                    else
                    {
                        texts[f].text = "X";
                        texts[f].color = Color.white;
                    }
                    f++;
                    break;
                }
            }
        }
    }

    void Update()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
