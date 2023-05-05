using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SO_Mission : ScriptableObject
{
    public string description;
    public float deliveryTimeMin;
    public float deliveryTimeMax;
    public int scoreValue, malusValue;
    public virtual void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        page.timer = Random.Range(deliveryTimeMin, deliveryTimeMax);
        page.deliveryTime = page.timer;
        page.activated = true;
        _missionChecker.color = Color.black;
        _missionText.color = Color.white;
        page.completed = false;
    }

    public virtual void OnCompleted(ref missionPage page, float scoreMult)
    {
        page.activated = false;
        if(page.completed) ScoreManager.Instance.ChangeScore((int)(scoreValue * scoreMult));
        else ScoreManager.Instance.ChangeScore(malusValue);
    }
}
