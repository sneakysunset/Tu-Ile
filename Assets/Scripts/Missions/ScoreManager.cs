using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score { get; private set; }
    public TextMeshProUGUI scoreText;
    public int[] scoreCaps;
    public int highscore;
    public bool isCompleted;
    public bool activated;
    [HideInInspector] public Image[] fillStars;



    public void ChangeScore(int _score)
    {
        score += _score;
        score = Mathf.Clamp(score, 0, 9999);
        scoreText.text = score.ToString();
        for (int i = 0; i < fillStars.Length; i++)
        {
            float scoreCapsMinus = 0;
            if(i > 0) scoreCapsMinus = (float)scoreCaps[(i - 1)];
            fillStars[i].fillAmount = ((float)score - scoreCapsMinus) / (float)scoreCaps[i];
            //DOVirtual.Float(fillStars[i].fillAmount, ((float)score - scoreCapsMinus) / (float)scoreCaps[i], 1, v => fillStars[i].fillAmount = v);
            //fillStars[i].fillAmount.
        }
    }
}
