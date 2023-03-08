using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vamos : MonoBehaviour
{
    public float speed = 3f;
    float refSpeed;
    public KeyCode keyResume;
    public KeyCode keyStop;
    private bool isVamos = true;

    private void Start()
    {
        refSpeed = speed;
    }
    private void Update()
    {
        //Deplacement
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        //Arreter le deplacement
        if (Input.GetKeyDown(keyStop) && isVamos)
        {
            StopVamos();
        }
        //Reprendre le deplacement
        if(Input.GetKeyDown(keyResume) && !isVamos)
        {
            ReVamos(refSpeed);
        }
    }
    public void StopVamos()
    {
        isVamos = false;
        speed = 0f;
        print(name + "est arrété");
    }
    public void ReVamos(float refSpeed)
    {
        isVamos = true;
        speed = refSpeed;
        print(name + "est en mouvement");
    }
}
