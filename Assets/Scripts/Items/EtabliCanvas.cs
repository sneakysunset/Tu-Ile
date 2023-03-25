using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using static UnityEditor.Progress;

public class EtabliCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Etabli etabli;
    TextMeshProUGUI text;
    Transform mainCamera;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        etabli = transform.parent.GetComponent<Item_Etabli>();
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        List<string> strings = new List<string>();
        for (int i = 0; i < etabli.requiredItemStacks.Length; i++)
        {
            if(etabli.requiredItemStacks[i].item.numberStacked > 0)
            {
                strings.Add(etabli.requiredItemStacks[i].stackType.ToString() + " : " + etabli.requiredItemStacks[i].item.numberStacked.ToString());
            }
        }
        text.text = string.Empty;
        if (strings.Count > 0)
        {
            foreach (string s in strings)
            {
                text.text += s + "\n";
            }
        }


        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        strings.Clear();
    }
}
