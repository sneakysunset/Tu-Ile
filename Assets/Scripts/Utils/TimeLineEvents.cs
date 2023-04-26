using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeLineEvents
{
    static public void ApocalypseEvent()
    {
        TileSystem tileS = TileSystem.Instance;
        List<Tile> targettedTiles = tileS.GetTilesAround(20, tileS.centerTile);
        foreach(Tile t in targettedTiles)
        {
            if (t.degradable)
            {
                t.currentPos.y -= t.heightByTile;
            }
        }
    }

    static public void AddMissionPage(MissionManager missionManager)
    {
        missionPage[] newPages = new missionPage[missionManager.activeMissions.Length + 1];
        for (int i = 0; i < missionManager.activeMissions.Length; i++)
        {
            newPages[i] = missionManager.activeMissions[i];
        }
        newPages[newPages.Length - 1] = new missionPage();
        newPages[newPages.Length - 1].missionUI = GameObject.Instantiate(missionManager.missionPrefab, missionManager.missionsFolder).GetComponent<RectTransform>();
        newPages[newPages.Length - 1].missionText = newPages[newPages.Length - 1].missionUI.GetComponentInChildren<TextMeshProUGUI>();
        newPages[newPages.Length - 1].missionChecker = newPages[newPages.Length - 1].missionUI.GetChild(0).GetChild(1).GetComponent<UnityEngine.UI.Image>();
        newPages[newPages.Length - 1].timer = 100;
        missionManager.activeMissions = newPages;
        missionManager.missionSlotNumber++;
        missionManager.StartCoroutine(missionManager.SetNewMission(newPages.Length - 1));

    }

    static public void ReduceMissionPool(MissionManager missionManager)
    {
        missionManager.RemoveMissionPoll();
    }

    static public void EnlargeMissionPool(MissionManager missionManager)
    {
        missionManager.AddMissionPool();
        missionManager.RemoveMissionPoll();
    }

    static public void ExtendMissionPool(MissionManager missionManager)
    {
        missionManager.AddMissionPool();
    }
}
