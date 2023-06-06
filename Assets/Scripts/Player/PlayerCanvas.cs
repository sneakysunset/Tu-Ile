using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCanvas : MonoBehaviour
{
    CameraCtr cam;
    Player player;
    TextMeshProUGUI text;
    Transform mainCamera;
    Image image;
    RessourcesManager rMan;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        player = GetComponentInParent<Player>();
        mainCamera = Camera.main.transform; 
        image = GetComponentInChildren<Image>();
        rMan = FindObjectOfType<RessourcesManager>();
    }

    void Update()
    {
        image.gameObject.SetActive(false);
        if (player.heldItem && (player.heldItem.GetType() == typeof(Item_Stack) || player.heldItem.GetType() == typeof(Item_Stack_Tile))) 
        {         
            Item_Stack itemS = player.heldItem as Item_Stack;
            text.text = "x " + itemS.numberStacked.ToString();
            image.gameObject.SetActive(true);
            Sprite sprite = null;
            bool isTile = false;
            if (player.heldItem.GetType() == typeof(Item_Stack_Tile)) isTile = true;
            foreach(ressourceMeshsCollec rMC in rMan.RessourceMeshs)
            {
                if(rMC.stackType == itemS.stackType && !isTile)
                {
                    sprite = rMC.sprite;
                    break;
                }
                else if(rMC.stackType == itemS.stackType && isTile)
                {
                    sprite = rMC.tileSprite;
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
