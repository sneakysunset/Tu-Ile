using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtr : MonoBehaviour
{
    public float smoother;
    private Vector3 velocity;
    private Camera cam;
    private Transform player;

    void Start()
    {
        cam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = player.position;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position, ref velocity, smoother);
    }
}
