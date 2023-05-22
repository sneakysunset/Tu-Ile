using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Scriptable Objects/BoMission", menuName = "Compass Mission", order = 1)]
public class SO_CompassM : ScriptableObject
{
    public int minDistanceFromCenter;
    public int maxDistanceFromCenter;

    public Item_Crate reward;

    public void OnActivated(bool isEphemeral, ref Tile targetTile)
    {
        if (!isEphemeral)
        {
            TileSystem tileSystem = TileSystem.Instance;
            List<Tile> tiles = GridUtils.GetTilesBetweenRaws(minDistanceFromCenter, maxDistanceFromCenter, tileSystem.centerTile);
            targetTile = tiles[Random.Range(0, tiles.Count)];
        }

        Item_Boussole[] items = FindObjectsOfType<Item_Boussole>();
        foreach (Item_Boussole item in items)
        {
            if (!item.targettedTiles.Contains(targetTile))
            {
                item.targettedTiles.Add(targetTile);
                item.UpdateTargettedList();
            }
        }
    }

    public void OnCompleted(Tile targettedTile)
    {
        Item_Boussole[] items = FindObjectsOfType<Item_Boussole>();
        foreach (Item_Boussole item in items)
        {
            item.targettedTiles.Remove(targettedTile);
            item.UpdateTargettedList();
        }
        Instantiate(reward, targettedTile.transform.GetChild(0).position, Quaternion.identity);
    }
}
