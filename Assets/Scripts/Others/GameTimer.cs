using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float gameTimer;
    public UnityEvent LevelEnd;
    public TextMeshProUGUI timerText;
    private void Start()
    {
       
    }

    private void Update()
    {
        gameTimer -= Time.deltaTime; 
        if(gameTimer <= 0)
        {
            LevelEnd?.Invoke();
        }


        int minutes = Mathf.FloorToInt(gameTimer / 60);
        if(minutes >= 1)
        {
            timerText.text = minutes.ToString() + " : " + Mathf.RoundToInt(gameTimer % 60).ToString();
        }
        else
        {
            timerText.text = Mathf.RoundToInt(gameTimer).ToString();
        }
    }
}
