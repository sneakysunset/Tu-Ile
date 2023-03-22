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
        text.text = "Wood : " + player.heldItems.Count.ToString();

        Vector3 dir = transform.position - Camera.main.transform.position;
        dir = dir.normalized;
        transform.forward = dir; ;
    }
}
