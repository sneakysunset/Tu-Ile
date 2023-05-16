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

    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        page.numOfKilledItem = 0;
        base.OnActivated(_missionChecker, _missionText, ref page);
        TimeLineEvents.InstantiateItems(itemToKill.gameObject, numToSpawn, radiusWidth, spawnHeight);
        _missionText.text = description;
        _missionText.text = description + " " + page.numOfKilledItem.ToString() + " / " + requiredNum.ToString();
    }
}
