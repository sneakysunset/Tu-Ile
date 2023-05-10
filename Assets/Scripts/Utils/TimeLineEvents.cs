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
        missionManager.activeMissions = newPages;
        missionManager.missionSlotNumber++;
        missionManager.SetMissionPage(missionManager.activeMissions.Length - 1);
    }

    static public void InstantiateItems(GameObject elTender, int numOfChicken, float spreadOfChicken, float spawnHeight)
    {
        int v = 0;
        while (v < numOfChicken)
        {
            Vector3 offSet = Random.insideUnitSphere * spreadOfChicken;
            offSet.y = 0;
            Vector3 pos = TileSystem.Instance.centerTile.transform.position + Vector3.up * spawnHeight + offSet;
            GameObject.Instantiate(elTender, pos, Quaternion.identity);
            v++;
        }
    }

    static public void AddEphemeralMission(MissionManager missionManager, SO_Mission ephemeralMission)
    {
        missionPage[] newPages = new missionPage[missionManager.activeMissions.Length + 1];
        for (int i = 0; i < missionManager.activeMissions.Length; i++)
        {
            newPages[i] = missionManager.activeMissions[i];
        }
        missionManager.activeMissions = newPages;
        missionManager.missionSlotNumber++;
        missionManager.SetMissionPage(missionManager.activeMissions.Length - 1);
        missionManager.activeMissions[missionManager.activeMissions.Length - 1].isEphemeral = true;
        missionManager.activeMissions[missionManager.activeMissions.Length - 1].potentialMission = ephemeralMission;
    }

    static public void RemoveMissionPage(MissionManager missionManager)
    {
        missionPage[] newPages = new missionPage[missionManager.activeMissions.Length - 1];
        for (int i = 0; i < newPages.Length; i++)
        {
            newPages[i] = missionManager.activeMissions[i];
        }
        GameObject.Destroy(missionManager.activeMissions[missionManager.activeMissions.Length - 1].missionUI.gameObject);
        missionManager.activeMissions = newPages;
        missionManager.missionSlotNumber--;
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
