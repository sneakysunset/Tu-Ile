using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class ChantierCanvas : MonoBehaviour
{
    CameraCtr cam;
    [HideInInspector] public Item_Etabli etabli;
    Transform mainCamera;
    [SerializeField] private GameObject[] layoutItems;
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private Image[] images;
    [SerializeField] private RectTransform fond;

    [SerializeField] private float baseFondHeight;
    [SerializeField] private float incrementalFondHeight;
    private RessourcesManager rMan;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        OnActivated();    
    }

    public void GetRessourceManagerInEditor() => rMan = FindObjectOfType<RessourcesManager>();

    public void OnActivated()
    {
        if (cam == null)
        {
            rMan = RessourcesManager.Instance;
            cam = TileSystem.Instance.cam;
        }

        fond.sizeDelta = new Vector2(fond.sizeDelta.x, baseFondHeight);

        for (int i = 0; i < layoutItems.Length; i++)
        {
            layoutItems[i].gameObject.SetActive(false);
        }

        int f = 0;
        for (int i = 0; i < etabli.recette.requiredItemStacks.Length; i++)
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == etabli.recette.requiredItemStacks[i].stackType)
                {
                    layoutItems[f].gameObject.SetActive(true);
                    images[f].sprite = rMC.sprite;
                    texts[f].text = etabli.recette.requiredItemStacks[i].cost.ToString();
                    fond.sizeDelta += incrementalFondHeight * Vector2.up;
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
                    layoutItems[f].gameObject.SetActive(true);
                    images[f].sprite = rMC.sprite;
                    texts[f].text = "V";
                    fond.sizeDelta += incrementalFondHeight * Vector2.up;
                    f++;
                }
            }
        }


    }

    public void UpdateText(Item_Etabli et)
    {
        int f = 0;
        if(cam == null)
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
                    texts[f].text = "x " + etabli.currentStackRessources[i] + " / " + etabli.recette.requiredItemStacks[i].cost.ToString();
                    if (etabli.currentStackRessources[i] >= etabli.recette.requiredItemStacks[i].cost)
                    {
                        texts[f].color = Color.green;
                    }
                    else
                    {
                        texts[f].color = Color.black;
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
                        texts[f].text = " V";
                        texts[f].color = Color.green;
                    }
                    else
                    {
                        texts[f].text = "X";
                        texts[f].color = Color.black;
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
        if(mainCamera) transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }


}
