using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/TileCreation", order = 1)]
public class SOM_Tile : SO_Mission
{
    public int requiredNumber;
    public Tile.TileType requiredType;


    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        base.OnActivated(_missionChecker, _missionText, ref page);
        TileCounter tileCounter = FindObjectOfType<TileCounter>();
        _missionText.text = description + " " + tileCounter.GetStat(requiredType).ToString() + " / " + requiredNumber.ToString();
    }
}
