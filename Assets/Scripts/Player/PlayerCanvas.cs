using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerCanvas : MonoBehaviour
{
    CameraCtr cam;
    private void Start()
    {
        cam = FindObjectOfType<CameraCtr>();
    }

    void Update()
    {
        Vector3 dir = cam.medianPos - Camera.main.transform.position;
        dir = dir.normalized;
        transform.forward = dir; ;
    }
}
