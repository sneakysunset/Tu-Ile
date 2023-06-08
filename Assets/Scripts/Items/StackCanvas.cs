using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StackCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Stack iS;
    TextMeshProUGUI text;
    Transform mainCamera;
    RessourcesManager rMan;
    Image image;
    private void Start()
    {

    }

    private void OnEnable()
    {
        iS = GetComponentInParent<Item_Stack>();
        cam = TileSystem.Instance.cam;
        text = GetComponentInChildren<TextMeshProUGUI>();
        if (Camera.main) mainCamera = Camera.main.transform;
        image = GetComponentInChildren<Image>();
        rMan = RessourcesManager.Instance;
        Sprite sprite = null;
        if(iS.GetType() == typeof(Item_Stack_Tile))
        {
            foreach (recetteResultCollec rMC in rMan.ressourceRecettesResults)
            {
                if (rMC.tileType == iS.stackType)
                {
                    sprite = rMC.sprite;
                    break;
                }
            }
        }
        else
        {
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == iS.stackType)
                {
                    sprite = rMC.sprite;
                    break;
                }
            }
        }
        image.sprite = sprite;
    }

    void OnNumberChanges()
    {

    }

    void Update()
    {
        
        if (iS.numberStacked > 0 && !iS.isHeld)
        {
            text.text = "x " + iS.numberStacked.ToString();
            image.gameObject.SetActive(true);
        }
        else
        {
            image.gameObject.SetActive(false);
            text.text = string.Empty;
        }
        if (mainCamera) transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
