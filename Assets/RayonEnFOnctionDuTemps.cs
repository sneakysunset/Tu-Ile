using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayonEnFOnctionDuTemps : MonoBehaviour
{
    public ParticleSystem[] rayons;
    public GameTimer gameTimer;
    private int timeNum = 6;
    public int numberOfRunes = 6;
    private int TimeNum { get { return timeNum; } set { if (timeNum != value) OnTimeStepChange(value); } }

    private void Start()
    {
        timeNum = numberOfRunes;    
    }

    private void Update()
    {
        if (gameTimer.gameTimer / numberOfRunes * timeNum >= gameTimer.timer)
        {
            TimeNum--;
        }
    }

    private void OnTimeStepChange(int value)
    {
        rayons[timeNum-1].Play();
        TimeNum = value;
    }
}
