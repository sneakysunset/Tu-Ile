using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Boussole", order = 1)]
public class SOM_Boussole : SO_Mission
{
    public int minDistanceFromCenter;
    public int maxDistanceFromCenter;

    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        base.OnActivated(_missionChecker, _missionText, ref page);
        _missionText.text = description;

        TileSystem tileSystem = TileSystem.Instance;
        List<Tile> tiles = tileSystem.GetTilesBetweenRaws(minDistanceFromCenter, maxDistanceFromCenter, tileSystem.centerTile);
        page.boussoleTile = tiles[Random.Range(0, tiles.Count)];
        Item_Boussole[] items = FindObjectsOfType<Item_Boussole>();
        foreach(Item_Boussole item in items)
        {
            if (!item.targettedTiles.Contains(page.boussoleTile))
            {
                item.targettedTiles.Add(page.boussoleTile);
                item.UpdateTargettedList();
            }
        }
    }

    public override void OnCompleted(ref missionPage page)
    {
        base.OnCompleted(ref page);

        Item_Boussole[] items = FindObjectsOfType<Item_Boussole>();
        foreach (Item_Boussole item in items)
        {
            item.targettedTiles.Remove(page.boussoleTile);
            item.UpdateTargettedList();
        }

        page.boussoleTile = null;

    }
}
