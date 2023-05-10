using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI ;
using System.Linq;

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
    public enum Events { Neutral, Apocalypse, AddMissionSlot, ExtendMissionPool, ReduceMissionPool, EnlargeMissionPool, EphemeralMission};
    public List<Event> events;
    public float gameTimer;
    [HideNormalInspector] public float timer;
    public UnityEvent LevelEnd;
    public Image timerFillImage;
    public Transform eventFolder;
    public GameObject eventVisualPrefab;
    public Sprite apocalypseImage;
    public Sprite newPageImage;
    public Sprite ephemeralMissionImage;
    private Player[] players;
    public Image minuteBar;
    public Image secondBar;
    public Transform minuteFolder;
    MissionManager missionManager;
    private void Start()
    {
        missionManager = MissionManager.Instance;
        //SortList();
        RectTransform r = timerFillImage.transform as RectTransform;

        foreach(Event e in events)
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

        }
        players = FindObjectsOfType<Player>();
    }

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

    private void Update()
    {
        if (TileSystem.Instance.ready)
        {
            GameTimerFunction();

            TimelineEvent();
        }
    }

    private void GameTimerFunction()
    {
        timer += Time.deltaTime;
        if (timer >= gameTimer)
        {
            LevelEnd?.Invoke();
            foreach(Player p in players)
            {
                p.respawnTile = players[0].tileUnder;
            }
            
            StartCoroutine(TileSystem.Instance.SinkWorld(players[0].tileUnder, "Hub"));
        }

        timerFillImage.fillAmount = timer / gameTimer;
    }

    private void TimelineEvent()
    {
        if (events.Count > 0 && events[0].eventTime < timer)
        {
            switch (events[0].timeLineEvent)
            {
                case Events.Apocalypse:
                    TimeLineEvents.ApocalypseEvent();
                    break;
                case Events.AddMissionSlot:
                    TimeLineEvents.AddMissionPage(missionManager);
                    break;
                case Events.ReduceMissionPool:
                    TimeLineEvents.ReduceMissionPool(missionManager);
                    break;
                case Events.ExtendMissionPool:
                    TimeLineEvents.ExtendMissionPool(missionManager);
                    break;
                case Events.EnlargeMissionPool:
                    TimeLineEvents.EnlargeMissionPool(missionManager);
                    break;
                case Events.EphemeralMission:
                    TimeLineEvents.AddEphemeralMission(missionManager, events[0].ephemeralMission);
                    break;
                default:
                    break;
            }
            events[0].eventTrigger?.Invoke();
            events.RemoveAt(0);
        }
    }
}
