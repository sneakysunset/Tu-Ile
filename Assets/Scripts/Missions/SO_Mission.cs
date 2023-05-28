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
    protected RessourcesManager ressourcesManager;
    public virtual void OnActivated(MissionUI mUIInGame, MissionUI mUIInPause, ref missionPage page)
    {
        if(ressourcesManager == null) ressourcesManager = FindObjectOfType<RessourcesManager>();    
        page.timer = Random.Range(deliveryTimeMin, deliveryTimeMax);
        page.deliveryTime = page.timer;
        page.activated = true;
        mUIInGame.missionText.color = Color.white;
        page.completed = false;
    }

    public virtual void OnCompleted(ref missionPage page, float scoreMult)
    {
        page.activated = false;
        if(page.completed) TileSystem.Instance.scoreManager.ChangeScore((int)(scoreValue * scoreMult));
        else TileSystem.Instance.scoreManager.ChangeScore(malusValue);
    }
}
