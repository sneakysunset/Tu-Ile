using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class Ship_CharacterController : MonoBehaviour
{
    //[HideInInspector] public CharacterController shipCharacterController;
    [HideInInspector] public Transform holdPoint;
    [HideInInspector] public bool isUsed;
    [HideNormalInspector] public Vector2 _input;
    public float smoothTime = 0.05f;
    private float _currentVelocity;
    public float speed;
    public float slowerSpeed = 2;
    public float acceleration;
    [HideInInspector] public Collider col;
    Rigidbody rb;
    private float timer;
    public float paddleFrequency = 1.2f;
    public float paddleStrength = 1;

    private void Start()
    {
       // shipCharacterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>(); 
        holdPoint = transform.Find("HoldPoint");
        col = GetComponent<Collider>();
        timer = paddleFrequency;
    }

    private void Update()
    {
        ApplyRotation();
        if (isUsed && _input.y != 0)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = paddleFrequency;
                rb.AddForce(paddleStrength * transform.forward, ForceMode.Impulse);
            }
        }
        else timer = paddleFrequency;
        ApplyMovement();
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;
        transform.Rotate(0, _input.x * Time.deltaTime * smoothTime, 0);
    }
    private void ApplyMovement()
    {
        //shipCharacterController.Move(_direction.z * transform.forward * speed * Time.deltaTime);

        rb.velocity = Vector3.Lerp(rb.velocity, _input.y * transform.forward * speed * Time.deltaTime, acceleration * Time.deltaTime);
        //_input = Vector2.Lerp(_input, Vector2.zero, slowerSpeed * Time.deltaTime) ;

    }

}
