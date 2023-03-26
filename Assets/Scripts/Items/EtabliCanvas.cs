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

        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
            texts[i].text = string.Empty;
        }
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            images[i].gameObject.SetActive(true);
        }
    }

    void Update()
    {
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    images[i].sprite = rMC.sprite;
                    texts[i].text = "x " + etabli.recette.requiredItemStacks[i].item.numberStacked + " / " + etabli.recette.requiredItemStacks[i].cost.ToString();
                    if(etabli.recette.requiredItemStacks[i].item.numberStacked >= etabli.recette.requiredItemStacks[i].cost)
                    {
                        texts[i].color = Color.green; 
                    }
                    else
                    {
                        texts[i].color = Color.white;
                    }
                    break;
                }
            }
        }


        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        //strings.Clear();
    }
}
