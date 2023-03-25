using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StackCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Stack iS;
    TextMeshProUGUI text;
    Transform mainCamera;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        iS = GetComponentInParent<Item_Stack>();
        mainCamera = Camera.main.transform;
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

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        //transform.forward = Vector3.forward;
    }
}
