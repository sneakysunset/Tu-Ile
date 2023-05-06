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
    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        base.OnActivated(_missionChecker, _missionText, ref page);
        _missionText.text = description;
        _missionText.text = description + " " + page.numOfKilledItem.ToString() + " / " + requiredNum.ToString();
    }
}
