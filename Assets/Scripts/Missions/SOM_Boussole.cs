using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Boussole", order = 1)]
public class SOM_Boussole : SO_Mission
{
    public Vector2Int targetTileCoord;

    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        base.OnActivated(_missionChecker, _missionText, ref page);
        _missionText.text = description;
    }
}
