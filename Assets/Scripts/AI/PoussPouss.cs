using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoussPouss : MonoBehaviour
{
    AI_Behaviour AIB;
    public float timerDeterioration;
    void Start()
    {
       AIB = GetComponent<AI_Behaviour>(); 
    }


    void Update()
    {
        if (AIB.tileUnder && AIB.tileUnder.walkedOnto && !AIB.tileUnder.isGrowing && AIB.tileUnder.degradable)
        {
            AIB.tileUnder.timer -= timerDeterioration * Time.deltaTime;
            AIB.tileUnder.isPenguined = true;
        }
    }
}
