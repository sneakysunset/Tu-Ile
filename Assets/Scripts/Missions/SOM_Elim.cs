using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Elimination", order = 1)]
public class SOM_Elim : SO_Mission
{
    public Item itemToKill;
    public int requiredNum;

    [Header("if no spawn keep numToSpawn at 0")]
    public int numToSpawn = 0;
    public float radiusWidth = 10;
    public float spawnHeight = 30;

    public override void OnActivated(MissionUI mUIInGame, MissionUI mUIInPause, ref missionPage page)
    {
        base.OnActivated(mUIInGame, mUIInPause, ref page);
        page.numOfKilledItem = 0;
        //mUIInGame.missionChecker.sprite = ressourcesManager.mChickenElim;
        //mUIInPause.missionChecker.sprite = ressourcesManager.mChickenElim;
        TimeLineEvents.InstantiateItems(itemToKill.gameObject, numToSpawn, radiusWidth, spawnHeight);
        mUIInGame.missionText.text = page.numOfKilledItem.ToString() + " / " + requiredNum.ToString();
        mUIInPause.missionText.text = description + " " + page.numOfKilledItem.ToString() + " / " + requiredNum.ToString();
    }
}
