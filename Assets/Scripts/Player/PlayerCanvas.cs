using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerCanvas : MonoBehaviour
{
    CameraCtr cam;
    Player player;
    TextMeshProUGUI text;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        player = transform.parent.GetComponent<Player>();
    }

    void Update()
    {
        if(player.heldItem != null && player.heldItem.GetType() == typeof(Item_Stack)) 
        {         
            Item_Stack itemS = player.heldItem as Item_Stack;
            text.text = "Wood : " + itemS.numberStacked.ToString();
        }
        else
        {
            text.text = string.Empty;
        }

        Vector3 dir = transform.position - Camera.main.transform.position;
        dir = dir.normalized;
        transform.forward = dir; ;
    }
}
