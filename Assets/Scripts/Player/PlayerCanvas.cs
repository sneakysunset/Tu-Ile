using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerCanvas : MonoBehaviour
{
    CameraCtr cam;
    Player player;
    TextMeshProUGUI text;
    Transform mainCamera;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        player = GetComponentInParent<Player>();
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (player.heldItem && player.heldItem.GetType() == typeof(Item_Stack)) 
        {         
            Item_Stack itemS = player.heldItem as Item_Stack;
            text.text = "Wood : " + itemS.numberStacked.ToString();
        }
        else
        {
            text.text = string.Empty;
        }

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
