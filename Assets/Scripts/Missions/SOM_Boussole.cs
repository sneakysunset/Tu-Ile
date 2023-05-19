using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Compass", order = 1)]
public class SOM_Boussole : SO_Mission
{
    public int minDistanceFromCenter;
    public int maxDistanceFromCenter;

    [Space(5)]
    [Header("If Ephemeral")]
    public Vector2Int tileCoordinates;

    public override void OnActivated(MissionUI mUIInGame, MissionUI mUIInPause, ref missionPage page)
    {
        base.OnActivated(mUIInGame ,mUIInPause, ref page);
        mUIInPause.missionText.text = description;
        mUIInGame.missionText.text = "";
        mUIInGame.missionChecker.sprite = ressourcesManager.mCompass;
        if(page.isEphemeral)
        {
            page.boussoleTile = TileSystem.Instance.tiles[tileCoordinates.x, tileCoordinates.y];
        }
        else
        {
            TileSystem tileSystem = TileSystem.Instance;
            List<Tile> tiles = GridUtils.GetTilesBetweenRaws(minDistanceFromCenter, maxDistanceFromCenter, tileSystem.centerTile);
            if(TileSystem.Instance.ready) page.boussoleTile = tiles[Random.Range(0, tiles.Count)];
        }
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

    public override void OnCompleted(ref missionPage page, float scoreMult)
    {
        base.OnCompleted(ref page, scoreMult);

        Item_Boussole[] items = FindObjectsOfType<Item_Boussole>();
        foreach (Item_Boussole item in items)
        {
            item.targettedTiles.Remove(page.boussoleTile);
            item.UpdateTargettedList();
        }

        page.boussoleTile = null;

    }
}
