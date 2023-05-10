using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable, HideInInspector]
public struct missionPage
{
    /*[HideInInspector]*/ public SO_Mission m;
    [HideInInspector] public RectTransform missionUI;
    [HideInInspector] public TextMeshProUGUI missionText;
    [HideInInspector] public Image missionFillBar;
    [HideInInspector] public Image missionFillBarOver;
    [HideInInspector] public Image missionChecker;
    [HideInInspector] public IEnumerator shakeCor;
    [HideInInspector] public IEnumerator lerpCor;
    [HideInInspector] public Item_Etabli chantier;
    [HideInInspector] public Tile boussoleTile;
    [HideInInspector] public bool tresorFound;
    [HideInInspector] public int numOfTileOnActivation;
    [HideInInspector] public int numOfKilledItem;
    [HideInInspector] public bool isReady;
    [HideInInspector] public int completionLevel;
    [HideInInspector] public bool completed;
    [HideInInspector] public bool isEphemeral;
    [HideInInspector] public SO_Mission potentialMission;
    
    public float timer;
    public float deliveryTime;
    public bool activated;
}

[System.Serializable]
public struct missionPool
{
    public SO_Mission[] missionList;
}

[System.Serializable]
public struct barColor
{
    public Color fillBarColor;
    [Range(0,1)] public float fillBarColorAmount;
    [Range(0, 2)] public float scoreMultiple;
}

public class MissionManager : MonoBehaviour
{
    #region variables
    [HideInInspector] public missionPage[] activeMissions;
    private TileCounter tileCounter;
    private WaitForEndOfFrame lerpWaiter;
    private WaitForSeconds shakeWaiter;
    private WaitForSeconds cooldownWaiter;

    [Header("Mission")]
    public missionPool[] missionPools;
    [HideInInspector] public List<SO_Mission> missionList;
    public int missionSlotNumber;
    public RectTransform missionsFolder;
    public GameObject missionPrefab;
    public barColor[] barColors;

    /*[HideInInspector]*/
    public int activePoolMin = 0;
    /*[HideInInspector]*/
    public int activePoolMax = -1;

    /*    [Space(10)]
        [Header("Mission Shake")]
        public AnimationCurve mPageShakingCurve;
        public float shakeMagnitude = 1;
        public Vector2 shakeValues;*/

    [Space(10)]
    [Header("Mission Lerps")]
    public float timeToComplete;
    public float missionCooldown;
    public AnimationCurve lerpEaseIn;
    #endregion

    #region System CallBacks

    public static MissionManager Instance;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    IEnumerator Start()
    {
        tileCounter = FindObjectOfType<TileCounter>();
        activeMissions = new missionPage[missionSlotNumber];
        missionList = new List<SO_Mission>();
        shakeWaiter = new WaitForSeconds(.08f);
        lerpWaiter = new WaitForEndOfFrame();
        cooldownWaiter = new WaitForSeconds(missionCooldown);
        yield return new WaitUntil(() => TileSystem.Instance.ready);
        AddMissionPool();
        SetMissionPages();
    }

    private void Update()
    {
        if (TileSystem.Instance.ready)
        {
            for (int i = 0; i < activeMissions.Length; i++)
            {
                if (activeMissions[i].m != null)
                {
                    if (activeMissions[i].isReady) activeMissions[i].timer -= Time.deltaTime;
                    activeMissions[i].timer = Mathf.Clamp(activeMissions[i].timer, 0, activeMissions[i].deliveryTime);

                    //print(1 - (activeMissions[i].timer / activeMissions[i].deliveryTime) + " " + activeMissions[i].completionLevel);
                    if (1 - (activeMissions[i].timer / activeMissions[i].deliveryTime) > barColors[activeMissions[i].completionLevel].fillBarColorAmount)
                    {
                        activeMissions[i].missionFillBarOver.color = barColors[activeMissions[i].completionLevel].fillBarColor;
                        activeMissions[i].completionLevel++;
                    }

                    if (activeMissions[i].timer <= 0 && activeMissions[i].m && activeMissions[i].activated)
                    {
                        if (activeMissions[i].lerpCor != null)
                        {
                            StopCoroutine(activeMissions[i].lerpCor);
                        }
                        activeMissions[i].lerpCor = SetNewMission(i);
                        StartCoroutine(activeMissions[i].lerpCor);
                    }
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
        Vector3 endPos = -Vector3.right * 700;
        activeMissions[missionIndex].isReady = false;
        if (activeMissions[missionIndex].m != null)
        {
            //Complete Previous Mission + Stop Mission Shaking
            rec.localPosition = startPos;

            activeMissions[missionIndex].m.OnCompleted(ref activeMissions[missionIndex], barColors[activeMissions[missionIndex].completionLevel].scoreMultiple);
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

            if (activeMissions[missionIndex].isEphemeral)
            {
                TimeLineEvents.RemoveMissionPage(this);
                yield break;
            }
        }
        if (!TileSystem.Instance.ready)
        {
            activeMissions[missionIndex].activated = false;
            activeMissions[missionIndex].timer = 100;
            yield break;
        }
        rec.localPosition = endPos;
        f = 1;

        yield return cooldownWaiter;
        activeMissions[missionIndex].missionFillBarOver.color = Color.gray;

        //Set up New Mission

        if (!activeMissions[missionIndex].isEphemeral) activeMissions[missionIndex].m = GetNewMission(activeMissions[missionIndex]);
        else activeMissions[missionIndex].m = activeMissions[missionIndex].potentialMission;
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
        activeMissions[missionIndex].isReady = true;
        activeMissions[missionIndex].completionLevel = 0;
        //Start Mission Shaking
        activeMissions[missionIndex].shakeCor = ShakeMission(missionIndex);
        StartCoroutine(activeMissions[missionIndex].shakeCor);
        activeMissions[missionIndex].lerpCor = null;
    }

    private void SetMissionPages()
    {
        for (int i = 0; i < activeMissions.Length; i++)
        {
            SetMissionPage(i);
        }
    }

    private SO_Mission GetNewMission(missionPage page)
    {
        SO_Mission m = null;
        int r = 0;
        do
        {
            r++;
            if (r > 100000)
            {
                Debug.LogError("NO MISSION AVAILABLE");
                throw new System.Exception();
            }

            m = missionList[Random.Range(0, missionList.Count)];
        }
        while (!CheckIfChantier(m, page) || !CheckIfTile(m, page) || !CheckIfElim(m, page));
        return m;
    }

    public void SetMissionPage(int i)
    {

        activeMissions[i] = new missionPage();
        activeMissions[i].missionUI = Instantiate(missionPrefab, missionsFolder).GetComponent<RectTransform>();
        activeMissions[i].missionText = activeMissions[i].missionUI.GetComponentInChildren<TextMeshProUGUI>();
        activeMissions[i].missionChecker = activeMissions[i].missionUI.GetChild(0).GetChild(1).GetComponent<Image>();
        activeMissions[i].missionFillBarOver = activeMissions[i].missionUI.GetChild(0).GetChild(3).GetChild(0).GetComponent<Image>();
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

    public void CloseMissions()
    {
        for (int i = 0; i < activeMissions.Length; i++)
        {
            if (activeMissions[i].lerpCor != null)
            {
                StopCoroutine(activeMissions[i].lerpCor);
            }
            activeMissions[i].lerpCor = CloseMission(i);
            StartCoroutine(activeMissions[i].lerpCor);
        }
    }

    IEnumerator CloseMission(int missionIndex)
    {
        float f = 0;
        RectTransform rec = activeMissions[missionIndex].missionUI.GetChild(0).GetComponent<RectTransform>();
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = -Vector3.right * 700;
        activeMissions[missionIndex].isReady = false;
        if (activeMissions[missionIndex].m != null)
        {
            //Complete Previous Mission + Stop Mission Shaking
            rec.localPosition = startPos;

            activeMissions[missionIndex].m.OnCompleted(ref activeMissions[missionIndex], barColors[activeMissions[missionIndex].completionLevel].scoreMultiple);
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
        activeMissions[missionIndex].activated = false;

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
                //case "SOM_ELim": StartCoroutine(); break;
        }

    }

    private IEnumerator CheckTileMission(SOM_Tile mission, missionPage page, int pageNum)
    {
        if (mission.requiredNumber <= tileCounter.GetStat(mission.requiredType) - page.numOfTileOnActivation)
        {
            page.missionChecker.color = Color.yellow;
            page.missionText.color = Color.yellow;
            activeMissions[pageNum].completed = true;
        }
        page.missionText.text = mission.description + " " + (tileCounter.GetStat(mission.requiredType) - page.numOfTileOnActivation).ToString() + " / " + mission.requiredNumber.ToString();

        if (tileCounter.GetStat(mission.requiredType) - page.numOfTileOnActivation >= mission.requiredNumber)
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
            activeMissions[pageNum].completed = true;
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
            activeMissions[pageNum].completed = true;
            yield return new WaitForSeconds(1);
            StartCoroutine(SetNewMission(pageNum));
        }
    }

    public IEnumerator CheckElimMission(System.Type type)
    {
        int pageNum = -1;
        SOM_Elim sE = null;
        for (int i = 0; i < activeMissions.Length; i++)
        {
            if (activeMissions[i].m != null && activeMissions[i].activated)
            {
                if (activeMissions[i].m.GetType() == typeof(SOM_Elim))
                {
                    pageNum = i;
                    sE = activeMissions[i].m as SOM_Elim; break;
                }
            }
        }
        if (pageNum == -1) yield return null;
        else
        {
            if (sE.itemToKill.GetType() == type)
            {
                activeMissions[pageNum].numOfKilledItem++;
                activeMissions[pageNum].missionText.text = sE.description + " " + activeMissions[pageNum].numOfKilledItem.ToString() + " / " + sE.requiredNum.ToString();
                if (activeMissions[pageNum].numOfKilledItem >= sE.requiredNum)
                {
                    activeMissions[pageNum].missionChecker.color = Color.yellow;
                    activeMissions[pageNum].missionText.color = Color.yellow;
                    activeMissions[pageNum].completed = true;
                    yield return new WaitForSeconds(1);
                    StartCoroutine(SetNewMission(pageNum));
                }
            }
        }

    }

    bool CheckIfChantier(SO_Mission m, missionPage page)
    {
        if (m.GetType() != typeof(SOM_Chantier)) return true;
        
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

    bool CheckIfTile(SO_Mission m, missionPage page)
    {
        if (m.GetType() != typeof(SOM_Tile)) return true;
        SOM_Tile mT = m as SOM_Tile;
        foreach (missionPage n in activeMissions)
        {
            if (n.m != null && n.m.GetType() == typeof(SOM_Tile))
            {
                var t = (SOM_Tile)n.m;

                if (t.requiredType == mT.requiredType)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool CheckIfElim(SO_Mission m, missionPage page)
    {
        if (m.GetType() != typeof(SOM_Elim)) return true;
        SOM_Elim mE = m as SOM_Elim;
        foreach (missionPage n in activeMissions)
        {
            if (n.m != null && n.m.GetType() == typeof(SOM_Elim))
            {
                var t = (SOM_Elim)n.m;
                if (t.itemToKill == mE.itemToKill)
                {
                    return false;
                }
            }
        }
        return true;
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

}
