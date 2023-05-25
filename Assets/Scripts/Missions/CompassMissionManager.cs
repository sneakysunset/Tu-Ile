using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BoussoleMission
{
    public SO_CompassM compassMission;
    public Tile targettedTile;
    public bool isEphemeral;
    public BoussoleMission(SO_CompassM compassMission, Tile targettedTile , bool isEphemeral)
    {
        this.compassMission = compassMission;
        this.targettedTile = targettedTile;
        this.isEphemeral = isEphemeral;
        this.compassMission.OnActivated(this.isEphemeral, ref this.targettedTile);
    }
}

public class CompassMissionManager : MonoBehaviour
{

    public List<SO_CompassM> compassMissions;
    public List<BoussoleMission> activeM;
    public int minCompassM, maxCompassM;
    public int compassMissionSlots;

    public static CompassMissionManager Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);

        }
        else
        {
            Instance = this;
        }
    }

    private IEnumerator Start()
    {
        activeM = new List<BoussoleMission> ();
        TileSelector.missionComplete += CompleteMission;
        yield return new WaitUntil(() => TileSystem.Instance.ready);
        for (int i = 0; i < compassMissionSlots; i++)
        {
            AddCompassMission(false);
        }
    }

    private void OnDisable()
    {
        TileSelector.missionComplete -= CompleteMission;
    }

    private void AddCompassMission(bool isEphemeral, Tile targettedTile = null)
    {
        SO_CompassM sCM = compassMissions[Random.Range(minCompassM, maxCompassM)];
        BoussoleMission bM = new BoussoleMission(sCM, targettedTile ,isEphemeral);
        activeM.Add(bM);
    }

    public void CompleteMission(Tile tile)
    {
        for (int i = 0; i < activeM.Count; i++)
        {
            if(activeM[i].targettedTile == tile)
            {
                activeM[i].compassMission.OnCompleted(activeM[i].targettedTile);
                if (!activeM[i].isEphemeral)
                {
                    AddCompassMission(false);
                }
                activeM.RemoveAt(i);
                break;
            }
        }
    }
}
