using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouvemenntDePersonnage : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * Time.deltaTime * speed);
    }
}
