using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayTime = 10;
    public float nightTime = 5;
    private float time;
    private WaitForSeconds waiter;
    public bool day = true;
    private TileMovements nSM;
    private void Start()
    {
        nSM = FindObjectOfType<TileMovements>();
        if (day)
        {
            time = dayTime;
            //nSM.isNearSighted = false;
        }
        else
        {
            time = nightTime;
           // nSM.isNearSighted = true;
        }
        waiter = new WaitForSeconds(time);
        StartCoroutine(dayCycleEnum());
               
    }

    private void Update()
    {
        transform.Rotate(Time.deltaTime * 180 / time, 0, 0);
    }

    IEnumerator dayCycleEnum()
    {
        yield return new WaitForSeconds(time);
        if (day)
        {
            day = false;
            NightEffect();
        }
        else
        {
            day = true;
            DayEffect();
        } 
        StartCoroutine(dayCycleEnum());
    }

    void DayEffect()
    {
        print("day");
        time = dayTime;
        //nSM.isNearSighted = false;
    }

    void NightEffect()
    {
        print("night");

        time = nightTime;
        //nSM.isNearSighted = true;
    }


}
