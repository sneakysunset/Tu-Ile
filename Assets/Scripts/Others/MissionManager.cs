using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct missionPage
{
    public float deliveryTimer;
    public List<SO_Mission> missions;
}

public class MissionManager : MonoBehaviour
{
    public List<missionPage> levelMissions;
    private TileCounter tileCounter;
    private GameTimer gmTimer;
    public Transform missionsFolder;
    public GameObject missionPrefab;
    public float paddingInBetween = 1;
    int missionPageIndex;
    float passingTimer;
    private void Start()
    {
        tileCounter = FindObjectOfType<TileCounter>();
        gmTimer = GetComponent<GameTimer>();
        SetNewMissions();   
    }

    private void Update()
    {
        if (levelMissions[missionPageIndex].deliveryTimer + passingTimer <= gmTimer.timer)
        {
            passingTimer += gmTimer.timer;
            missionPageIndex++;
            SetNewMissions();
        }
    }

    private void SetNewMissions()
    {
        float padding = 0;
        if (missionsFolder.childCount > 0)
        {
            for (int i = missionsFolder.childCount - 1; i >= 0 ; i--)
            {
                Destroy(missionPrefab.transform.GetChild(i).gameObject);
            }
        }

        foreach (SO_Mission s in levelMissions[missionPageIndex].missions)
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
        foreach (SO_Mission s in levelMissions[missionPageIndex].missions)
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
