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
        iS = GetComponentInParent<Item_Stack>();
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        mainCamera = Camera.main.transform;
        image = GetComponentInChildren<Image>();
        rMan = FindObjectOfType<RessourcesManager>();
    }

    void Update()
    {
        image.gameObject.SetActive(false);
        if (iS.numberStacked > 0 && !iS.isHeld)
        {
            text.text = "x " + iS.numberStacked.ToString();
            image.gameObject.SetActive(true);
            Sprite sprite = null;
            foreach (ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if (rMC.stackType == iS.stackType)
                {
                    sprite = rMC.sprite;
                    break;
                }
            }
            image.sprite = sprite;
        }
        else
        {
            text.text = string.Empty;
        }

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
