using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using static UnityEditor.Progress;

public class EtablieCanvas : MonoBehaviour
{
    CameraCtr cam;
    Item_Etablie etablie;
    TextMeshProUGUI text;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        etablie = transform.parent.GetComponent<Item_Etablie>();
    }

    void Update()
    {
        List<string> strings = new List<string>();
        for (int i = 0; i < etablie.requiredItemStacks.Length; i++)
        {
            if(etablie.requiredItemStacks[i].item.numberStacked > 0)
            {
                strings.Add(etablie.requiredItemStacks[i].stackType.ToString() + " : " + etablie.requiredItemStacks[i].item.numberStacked.ToString());
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


        Vector3 dir = transform.position - Camera.main.transform.position;
        dir = dir.normalized;
        transform.forward = dir; ;
        strings.Clear();
    }
}
