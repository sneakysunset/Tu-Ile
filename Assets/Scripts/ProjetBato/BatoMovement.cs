using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatoMovement : MonoBehaviour
{
    private Rigidbody _body;
    public float speed = 3f;
    private Vector3 desiredLocation = Vector3.zero;
    public KeyCode keyStop;
    public KeyCode keyResume;
    private float refSpeed;
    public float maxSpeed = 4f;
    public float decrease = 0.1f;
    public float refDecrease;
    public bool isMoving = true;
    public bool stopDecrease;
    private float t;
    [Range(0.1f, 200f)] public float rateDecrease = 1f;
    // Start is called before the first frame update
    void Awake()
    {
        _body = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        refSpeed = speed;
        refDecrease = decrease;
    }
    private void Update()
    {
        desiredLocation = Vector3.zero;
        desiredLocation = Vector3.right;
        //Start et Stop le deplacement du bateau
        if (Input.GetKeyDown(keyStop) && isMoving)
        {
            StopVamos();
        }
        //Reprendre le deplacement
        if (Input.GetKeyDown(keyResume) && !isMoving)
        {
            ReVamos(refSpeed);
        }
        //Timer qui réduit la vitesse du bateau
        if (isMoving)
        {
            t += Time.deltaTime;
            if(t >= rateDecrease)
            {
                speed -= decrease;
                t = 0f;
            }
            //Limitation de la Vitesse
            if (speed <= 0f)
            {
                speed = 0f;
            }
            if(speed>= maxSpeed)
            {
                speed = maxSpeed;
            }
        }
    }
    public void StopVamos()
    {
        t = 0f;
        isMoving = false;
        refSpeed = speed;
        speed = 0f;
        print(name + "est arrété");      
    }
    public void ReVamos(float refSpeed)
    {
        isMoving = true;
        speed = refSpeed;
        print(name + "est en mouvement");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _body.MovePosition(_body.position + desiredLocation * speed * Time.fixedDeltaTime);
    }
}
