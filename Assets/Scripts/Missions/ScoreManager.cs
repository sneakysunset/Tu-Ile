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
    [HideInInspector] public Image fillStar;



    public void ChangeScore(int _score)
    {
        score += _score;
        score = Mathf.Clamp(score, 0, 9999);
        scoreText.text = score.ToString();
        fillStar.fillAmount = (float)score / (float)scoreCaps[2];
    }
}
