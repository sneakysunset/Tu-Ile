using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/Chantier", order = 1)]
public class SOM_Chantier : SO_Mission
{
    public Item_Etabli chantierPrefab;
    public override void OnActivated(MissionUI mUIInGame, MissionUI mUIInPause, ref missionPage page)
    {
        base.OnActivated(mUIInGame, mUIInPause, ref page);
        mUIInGame.missionChecker.sprite = ressourcesManager.mConstr;
        mUIInPause.missionChecker.sprite = ressourcesManager.mConstr;
        List<Tile> ts = TileSystem.Instance.GetTilesAround(4, TileSystem.Instance.centerTile);
        Tile tile = null;
        for (int i = ts.Count - 1; i >= 0 ; i--)
        {
            if (ts[i].walkable && ts[i].tileSpawnType == TileType.construction)
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
        tile.tileSpawnType = TileType.Neutral;
        mUIInGame.missionText.text = "";
        mUIInPause.missionText.text = description;
    }

    public override void OnCompleted(ref missionPage page, float scoreMult)
    {
        base.OnCompleted(ref page, scoreMult);
        if(page.chantier && !page.chantier.constructed)
        {
            page.chantier.OnDestroyMethod();
            Destroy(page.chantier.gameObject);
        }
    }
}
