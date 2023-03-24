using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StackCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Stack iS;
    TextMeshProUGUI text;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        iS = GetComponentInParent<Item_Stack>();
    }

    void Update()
    {
        if (!iS.isHeld && iS.holdable)
        {
            text.text = iS.stackType.ToString() + " : " + iS.numberStacked.ToString();
        }   
        else
        {
            text.text = string.Empty;
        }

        Vector3 dir = transform.position - Camera.main.transform.position;
        dir = dir.normalized;
        dir.y = 0;
        transform.forward = dir; ;
        //transform.forward = Vector3.forward;
    }
}
