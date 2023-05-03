using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Chantier", order = 1)]
public class SOM_Chantier : SO_Mission
{
    public Item_Etabli chantierPrefab;
    public override void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        base.OnActivated(_missionChecker, _missionText, ref page);
        List<Tile> ts = TileSystem.Instance.GetTilesAround(4, TileSystem.Instance.centerTile);
        Tile tile = null;
        for (int i = ts.Count - 1; i >= 0 ; i--)
        {
            if (ts[i].walkable && ts[i].tileSpawnType == Tile.TileType.construction)
            {
                tile = ts[i];
            }
        }
        if (tile == null)
        {
            page.timer = 0;
            return;
        }
        page.chantier = Instantiate(chantierPrefab, tile.transform.position + Vector3.up * GameConstant.tileHeight, Quaternion.identity);
        tile.tileSpawnType = Tile.TileType.Neutral;
        _missionText.text = description;
    }

    public override void OnCompleted(ref missionPage page)
    {
        base.OnCompleted(ref page);
        if(page.chantier && !page.chantier.constructed)
        {
            Destroy(page.chantier.gameObject);
        }
    }
}
