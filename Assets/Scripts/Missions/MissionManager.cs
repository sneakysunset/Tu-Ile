using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct missionPage
{
    /*[HideInInspector]*/ public SO_Mission m;
    [HideInInspector] public RectTransform missionUI;
    [HideInInspector] public TextMeshProUGUI missionText;
    [HideInInspector] public Image missionFillBar;
    [HideInInspector] public Image missionChecker;
    [HideInInspector] public IEnumerator shakeCor;
    [HideInInspector] public Item_Etabli chantier;
    [HideInInspector] public Tile boussoleTile;
    [HideInInspector] public bool tresorFound;
    public float timer;
    public float deliveryTime;
    public bool activated;
}

[System.Serializable]
public struct missionPool
{
    public SO_Mission[] missionList;
}

public class MissionManager : MonoBehaviour
{
    #region variables
    /*[HideInInspector]*/ public missionPage[] activeMissions;
    private TileCounter tileCounter;
    private WaitForEndOfFrame lerpWaiter;
    private WaitForSeconds shakeWaiter;

    [Header("Mission")]
    public missionPool[] missionPools;
    /*[HideInInspector]*/ public List<SO_Mission> missionList;
    public int missionSlotNumber;
    public RectTransform missionsFolder;
    public GameObject missionPrefab;

    /*[HideInInspector]*/ public int activePoolMin = 0;
    /*[HideInInspector]*/ public int activePoolMax = -1;

    [Space(10)]
    [Header("Mission Shake")]
    public AnimationCurve mPageShakingCurve;
    public float shakeMagnitude = 1;
    public Vector2 shakeValues;

    [Space(10)]
    [Header("Mission Lerps")]
    public float timeToComplete;
    public AnimationCurve lerpEaseIn;
    #endregion

    #region System CallBacks
    private void Start()
    {
        tileCounter = FindObjectOfType<TileCounter>();
        activeMissions = new missionPage[missionSlotNumber];
        missionList = new List<SO_Mission>();
        AddMissionPool();
        SetMissionPages();
        shakeWaiter = new WaitForSeconds(.08f);
        lerpWaiter = new WaitForEndOfFrame();
    }

    private void Update()
    {
        for (int i = 0; i < activeMissions.Length; i++)
        {
            if (activeMissions[i].m != null)
            {
                activeMissions[i].timer -= Time.deltaTime;
                if (activeMissions[i].timer < 0 && activeMissions[i].m && activeMissions[i].activated)
                {
                    StartCoroutine(SetNewMission(i));
                }
            }
        }
    }
    #endregion

    #region Setting Up Missions
    public IEnumerator SetNewMission(int missionIndex)
    {
        float f = 0;
        RectTransform rec = activeMissions[missionIndex].missionUI.GetChild(0).GetComponent<RectTransform>();
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = - Vector3.right * 700;
        if (activeMissions[missionIndex].m != null)
        {
            //Complete Previous Mission + Stop Mission Shaking
            rec.localPosition = startPos;
            activeMissions[missionIndex].m.OnCompleted(ref activeMissions[missionIndex]);
            StopCoroutine(activeMissions[missionIndex].shakeCor);
            //Lerp Out Of Screen
            while (f < 1f)
            {
                f += Time.deltaTime * (1 / timeToComplete);

                float lerpValue;
                lerpValue = lerpEaseIn.Evaluate(f);

                rec.localPosition = Vector3.Lerp(startPos, endPos, lerpValue);

                yield return lerpWaiter;
            }
        }

        rec.localPosition = endPos;
        f = 1;

        //Set up New Mission
        SO_Mission m = missionList[Random.Range(0, missionList.Count)];
        if (m.GetType() == typeof(SOM_Chantier) && !CheckIfChantier())
        {
            do
            {
                m = missionList[Random.Range(0, missionList.Count)];
            }
            while (m.GetType() == typeof(SOM_Chantier)) ;
        }

        activeMissions[missionIndex].m = m;
        activeMissions[missionIndex].m.OnActivated(activeMissions[missionIndex].missionChecker, activeMissions[missionIndex].missionText, ref activeMissions[missionIndex]);
        activeMissions[missionIndex].timer = activeMissions[missionIndex].deliveryTime;
        activeMissions[missionIndex].missionFillBar.fillAmount = 0;
        //Lerp Into Screen
        while (f > 0f)
        {
            f -= Time.deltaTime * (1 / timeToComplete);

            float lerpValue;
            lerpValue = lerpEaseIn.Evaluate(f);

            rec.localPosition = Vector3.Lerp(startPos, endPos, lerpValue);

            yield return lerpWaiter;
        }

        rec.localPosition = startPos;

        //Start Mission Shaking
        activeMissions[missionIndex].shakeCor = ShakeMission(missionIndex);
        StartCoroutine(activeMissions[missionIndex].shakeCor);
    }

    private void SetMissionPages()
    {
        for (int i = 0; i < activeMissions.Length; i++)
        {
            SetMissionPage(i);
        }
    }

    public void SetMissionPage(int i)
    {

        activeMissions[i] = new missionPage();
        activeMissions[i].missionUI = Instantiate(missionPrefab, missionsFolder).GetComponent<RectTransform>();
        activeMissions[i].missionText = activeMissions[i].missionUI.GetComponentInChildren<TextMeshProUGUI>();
        activeMissions[i].missionChecker = activeMissions[i].missionUI.GetChild(0).GetChild(1).GetComponent<Image>();
        activeMissions[i].missionFillBar = activeMissions[i].missionUI.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetComponent<Image>();
        activeMissions[i].timer = 100;


        StartCoroutine(SetNewMission(i));
    }

    public void AddMissionPool()
    {
        activePoolMax++;
        foreach (var item in missionPools[activePoolMax].missionList)
        {
            missionList.Add(item);
        }
    }

    public void RemoveMissionPoll()
    {
        foreach (var item in missionPools[activePoolMin].missionList)
        {
            missionList.Remove(item);
        }
        activePoolMin++;
    }
    #endregion

    #region CheckingMissions
    public void CheckMissions()
    {
        for (int i = 0; i < activeMissions.Length; i++)
        {
            CheckMissionCompletion(activeMissions[i], i);
        }
    }

    private void CheckMissionCompletion(missionPage page, int pageNum)
    {
        switch (page.m.GetType().ToString())
        {
            case "SOM_Tile": StartCoroutine(CheckTileMission(page.m as SOM_Tile, page, pageNum)); break;
            case "SOM_Chantier": StartCoroutine(CheckChantierMission(page.m as SOM_Chantier, page, pageNum)); break;
            case "SOM_Boussole": StartCoroutine(CheckBoussoleMission(page.m as SOM_Boussole, page, pageNum)); break;
        }
        
    }

    private IEnumerator CheckTileMission(SOM_Tile mission, missionPage page, int pageNum)
    {
        if (mission.requiredNumber <= tileCounter.GetStat(mission.requiredType))
        {
            page.missionChecker.color = Color.yellow;
            page.missionText.color = Color.yellow;
        }
        page.missionText.text = mission.description + " " + tileCounter.GetStat(mission.requiredType).ToString() + " / " + mission.requiredNumber.ToString();

        if(tileCounter.GetStat(mission.requiredType) >= mission.requiredNumber)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(SetNewMission(pageNum));
        }
    }

    private IEnumerator CheckChantierMission(SOM_Chantier mission, missionPage page, int pageNum)
    {
        if (page.chantier != null && page.chantier.constructed)
        {
            page.missionChecker.color = Color.yellow;
            page.missionText.color = Color.yellow;
            yield return new WaitForSeconds(1);
            StartCoroutine(SetNewMission(pageNum));
        }
        else if (!page.chantier || page.chantier.isDestroyed)
        {
            activeMissions[pageNum].timer = 0;
        }
    }

    private IEnumerator CheckBoussoleMission(SOM_Boussole mission, missionPage page, int pageNum)
    {
        if (page.boussoleTile != null && page.tresorFound)
        {
            page.missionChecker.color = Color.yellow;
            page.missionText.color = Color.yellow;
            yield return new WaitForSeconds(1);
            StartCoroutine(SetNewMission(pageNum));
        }
    }
    #endregion

    IEnumerator ShakeMission(int mI)
    {
        RectTransform rec = activeMissions[mI].missionUI.GetChild(0).GetComponent<RectTransform>();
        yield return new WaitForEndOfFrame();
        Vector3 ogPos = rec.localPosition;
        SO_Mission m = activeMissions[mI].m;
        //bool yo = false;
        while (activeMissions[mI].m = m)
        {
            /*            float i = mPageShakingCurve.Evaluate(activeMissions[mI].timer / activeMissions[mI].deliveryTime);
                        *//*            float x = Random.Range(-i, i) * shakeMagnitude;
                                    float y = Random.Range(-i, i) * shakeMagnitude;*//*
                        float x = 0;
                        float y = 0;
                        if(yo)
                        {
                            x = -i * shakeMagnitude * shakeValues.x;
                            y = -i * shakeMagnitude * shakeValues.y;
                            yo = false;
                        }
                        else
                        {
                            x = i * shakeMagnitude * shakeValues.x;
                            y = i * shakeMagnitude * shakeValues.y;
                            yo = true;
                        }*/
            //rec.localPosition = new Vector3(ogPos.x + x, ogPos.y + y, ogPos.z);
            float i = activeMissions[mI].timer / activeMissions[mI].deliveryTime;
            activeMissions[mI].missionFillBar.fillAmount = 1 - i;

            yield return lerpWaiter;
        }

        rec.localPosition = ogPos;
    }

    bool CheckIfChantier()
    {
        List<Tile> ts = TileSystem.Instance.GetTilesAround(4, TileSystem.Instance.centerTile);
        for (int i = ts.Count - 1; i >= 0; i--)
        {
            if (ts[i].walkable && ts[i].tileSpawnType == Tile.TileType.construction)
            {
                return true;
            }
        }
        return false;
    }
}
