using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score { get; private set; }
    public TextMeshProUGUI scoreText;
        public void ChangeScore(int _score)
    {
        score += _score;
        score = Mathf.Clamp(score, 0, 9999);
        scoreText.text = score.ToString();
    }
}
