using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vamos : MonoBehaviour
{
    public float speed = 3f;
    float refSpeed;
    public KeyCode key;
    private bool isVamos = true;

    private void Start()
    {
        refSpeed = speed;
    }
    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (Input.GetKeyDown(key)&&isVamos)
        {
            StopVamos();
        }
        if(Input.GetKeyDown(key) && isVamos)
        {
            ReVamos(refSpeed);
        }
    }
    public void StopVamos()
    {
        isVamos = false;
        speed = 0;
    }
    public void ReVamos(float speedo)
    {
        isVamos = true;
        speed = speedo;
    }
}
