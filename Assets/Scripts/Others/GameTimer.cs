using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI ;
using System.Linq;
using UnityEditor;
using TMPro;

[System.Serializable]
public class Event
{
    public GameTimer.Events timeLineEvent;
    public float eventTime;
    public UnityEvent eventTrigger;
    [Header("FOR EPHEMERAL ONLY")]
    public SO_Mission ephemeralMission;
}



public class GameTimer : MonoBehaviour
{
    [HideInInspector] public LoadScene canvasRef { get; private set; }
    public enum Events { Neutral/*, Apocalypse, AddMissionSlot, ExtendMissionPool, ReduceMissionPool, EnlargeMissionPool, EphemeralMission, */, AddBoussoleMissionSlot, UpBoussoleMissionPool, AddBoussoleMissionPool, ReduceBoussoleMissionPool };
    public List<Event> events;
    public float gameTimer;
    [HideNormalInspector] public float timer;
    public UnityEvent LevelEnd;
    public ScoreManager scoreMan;
    public CompassMissionManager compassMan;
    //[HideNormalInspector] public string sceneLoadName = "Hub";
    //public Sprite apocalypseImage;
    //public Sprite newPageImage;
    //public Sprite ephemeralMissionImage;
    //public GameObject eventVisualPrefab;

    private void Awake()
    {
        canvasRef = FindObjectOfType<LoadScene>();    
        scoreMan = GetComponent<ScoreManager>();
        scoreMan.fillStar = canvasRef.fillStar;
        scoreMan.scoreText = canvasRef.scoreText;
        scoreMan.ChangeScore(0);
    }

    /*    private void Start()
        {
            //UIPos = FindObjectOfType<LoadScene>().transform.Find("TimeBar").GetComponent<RectTransform>();
            //timerFillImage = UIPos.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>();
            //eventFolder = timerFillImage.transform.parent.GetChild(2);
            //minuteFolder = timerFillImage.transform.parent.GetChild(0);
            //missionManager = MissionManager.Instance;
            //SortList();
            //RectTransform r = timerFillImage.transform as RectTransform;

            *//*foreach(Event e in events)
            {
                RectTransform t;
                switch (e.timeLineEvent)
                {
                    case Events.Apocalypse:
                        t = Instantiate(eventVisualPrefab, eventFolder).transform as RectTransform;
                        t.localPosition += new Vector3((e.eventTime / gameTimer) * 2 * (r.sizeDelta.x * r.localScale.x), 0, 0);
                        t.GetComponent<Image>().sprite = apocalypseImage;
                        break;
                    case Events.AddMissionSlot:
                        t = Instantiate(eventVisualPrefab, eventFolder).transform as RectTransform;
                        t.localPosition += new Vector3((e.eventTime / gameTimer) * 2 * (r.sizeDelta.x * r.localScale.x), 0, 0);
                        t.GetComponent<Image>().sprite = newPageImage;
                        break;
                    case Events.EphemeralMission:
                        t = Instantiate(eventVisualPrefab, eventFolder).transform as RectTransform;
                        t.localPosition += new Vector3((e.eventTime / gameTimer) * 2 * (r.sizeDelta.x * r.localScale.x), 0, 0);
                        t.GetComponent<Image>().sprite = ephemeralMissionImage;
                        break;
                    default:
                        break;
                }

            }*//*
        }*/


    private void SortList()
    {
        for (int i = 0; i < events.Count - 1; i++)
        {
            int smallestNumberIndex = i;
            for (int j = 0; j < events.Count; j++)
            {
                if (events[j].eventTime < events[smallestNumberIndex].eventTime)
                {
                    smallestNumberIndex = j;
                }
                if(smallestNumberIndex != i)
                {
                    Event temp = events[i];
                    events[i] = events[smallestNumberIndex];
                    events[smallestNumberIndex] = temp;
                }
            }
        }
    }


    public IEnumerator LerpTimeLine(Vector3 startPos, Vector3 endPos, RectTransform tr, AnimationCurve curve, float speed)
    {
        float t = 0;
        tr.anchoredPosition = startPos;
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        while(t < 1)
        {
            t += Time.deltaTime * (1/speed);
            tr.anchoredPosition = Vector3.Lerp(startPos, endPos, curve.Evaluate(t));
            yield return waiter;
        }
        tr.anchoredPosition = endPos;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            timer = gameTimer - 1;
        }

        if (TileSystem.Instance.ready)
        {
            GameTimerFunction();

            TimelineEvent();
        }
        if(Input.GetKeyDown(KeyCode.V)) timer = gameTimer - 5;
        if(Input.GetKeyDown(KeyCode.B)) timer = 100 - 5;
    }

    bool to;
    private void GameTimerFunction()
    {
        timer += Time.deltaTime;
        timer = Mathf.Clamp(timer, 0, gameTimer);
        if (timer >= gameTimer && !to)
        {
            to = true;
            StartCoroutine(FindObjectOfType<EndMenu>().EnableEnd());
        }
        canvasRef.timerText.text = Mathf.RoundToInt((gameTimer - timer)).ToString();
        canvasRef.timerFillImage.fillAmount = 1 - (timer / gameTimer);
    }

    public void EndLevel(bool isEnd, bool toHub)
    {
        events.Clear();
        LevelEnd?.Invoke();

        TileSystem.Instance.StartCoroutine(GridUtils.SinkWorld(TileSystem.Instance.centerTile, isEnd, toHub));
    }

    private void TimelineEvent()
    {
        if (events.Count > 0 && events[0].eventTime < timer)
        {
            switch (events[0].timeLineEvent)
            {
                case Events.AddBoussoleMissionSlot:
                    TimeLineEvents.AddBoussoleMissionSlot(compassMan);
                    break;
                case Events.UpBoussoleMissionPool:
                    TimeLineEvents.UpBoussoleMissionPool(compassMan);
                    break;
                case Events.AddBoussoleMissionPool:
                    TimeLineEvents.AddBoussoleMissionPool(compassMan);
                    break;
                case Events.ReduceBoussoleMissionPool:
                    TimeLineEvents.ReduceBoussoleMissionPool(compassMan);
                    break;
                //case Events.Apocalypse:
                    //TimeLineEvents.ApocalypseEvent();
                 //   break;
               // case Events.AddMissionSlot:
                    //TimeLineEvents.AddMissionPage(missionManager);
                   // break;
                //case Events.ReduceMissionPool:
                    //TimeLineEvents.ReduceMissionPool(missionManager);
                  //  break;
                //case Events.ExtendMissionPool:
                    //TimeLineEvents.ExtendMissionPool(missionManager);
                  //  break;
                //case Events.EnlargeMissionPool:
                    //TimeLineEvents.EnlargeMissionPool(missionManager);
                  //  break;
                //case Events.EphemeralMission:
                    //TimeLineEvents.AddEphemeralMission(missionManager, events[0].ephemeralMission);
                  //  break;
                default:
                    break;
            }
            events[0].eventTrigger?.Invoke();
            events.RemoveAt(0);
        }
    }
}
