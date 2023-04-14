using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    public List<SO_Mission> levelMissions;
    private TileCounter tileCounter;
    public Transform missionsFolder;
    public GameObject missionPrefab;
    public float paddingInBetween = 1;
    private void Start()
    {

        tileCounter = FindObjectOfType<TileCounter>();

        float padding = 0; 
        foreach (SO_Mission s in levelMissions)
        {
            Transform m = Instantiate(missionPrefab, missionsFolder).transform;
            s.missionText = m.GetComponentInChildren<TextMeshProUGUI>();
            s.missionChecker = m.GetComponentInChildren<Image>();
            s.missionText.text = s.description + " " + tileCounter.GetStat(s.requiredType).ToString() + " / " + s.requiredNumber.ToString();
            m.position += padding * Vector3.up;
            padding -= paddingInBetween;
        }
    }
    
    public void CheckMissions()
    {
        foreach (SO_Mission s in levelMissions)
        {
            CheckMissionCompletion(s);
        }
    }

    private void CheckMissionCompletion(SO_Mission mission)
    {
        if(mission.requiredNumber <= tileCounter.GetStat(mission.requiredType))
        {
            mission.missionChecker.color = Color.yellow;
            mission.missionText.color = Color.yellow;
        }
        else
        {
            mission.missionText.color = Color.black;
            mission.missionChecker.color = Color.black;
        }
        mission.missionText.text = mission.description + " " + tileCounter.GetStat(mission.requiredType).ToString() + " / " + mission.requiredNumber.ToString();
    }
}
