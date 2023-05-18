using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChantierCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Etabli etabli;
    TextMeshProUGUI[] texts;
    Transform mainCamera;
    Image[] images;
    private RessourcesManager rMan;


    private void OnActivated()
    {
        rMan = FindObjectOfType<RessourcesManager>();
        cam = FindObjectOfType<CameraCtr>();
        texts = GetComponentsInChildren<TextMeshProUGUI>();
        if (mainCamera) mainCamera = Camera.main.transform;
        images = GetComponentsInChildren<Image>();
        images[0].rectTransform.localScale = new Vector3(images[0].rectTransform.localScale.x, 0, images[0].rectTransform.localScale.z);
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
                    images[0].rectTransform.localScale += .35f * Vector3.up;
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
                    images[0].rectTransform.localScale += .35f * Vector3.up;
                    f++;
                }
            }
        }


    }

    public void UpdateText(Item_Etabli et)
    {
        int f = 0;
        if (etabli == null || rMan == null)
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
        if(mainCamera) transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
