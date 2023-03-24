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
        player = GetComponentInParent<Player>();
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

        Vector3 dir = cam.medianPos - Camera.main.transform.position;
        dir = dir.normalized;
        dir.y = 0;
        transform.forward = dir;
        //transform.LookAt(Camera.main.transform.position);
        //transform.localEulerAngles = new Vector3(0, 180, 0);
    }
}
