using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouvemenntDePersonnage : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 5f;
    Vector2 moveValue;
    [Range(-10f,10f)] 
    public float forceDeGravite = 1f;
    Vector3 move;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 move = new Vector3(moveValue.x, 0, moveValue.y) * Time.deltaTime* speed;
        if (controller.isGrounded&&controller.velocity.y>0f)
        {
            move.y = -1f;
        }
        else
        {
            move.y -= 9.81f * forceDeGravite * Time.deltaTime;
        }
        controller.Move(move);
        if (moveValue.x !=0 || moveValue.y !=0)
            transform.forward = new Vector3(moveValue.x, 0, moveValue.y);
    }
    public void OnDeplacement(InputAction.CallbackContext cbx)
    {
        moveValue = cbx.ReadValue<Vector2>();
    }
}
