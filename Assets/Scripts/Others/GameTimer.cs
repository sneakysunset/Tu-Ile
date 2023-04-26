using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI ;
using System.Linq;

[System.Serializable]
public struct Event
{
    public GameTimer.Events timeLineEvent;
    public float eventTime;
    public UnityEvent eventTrigger;
}
public class GameTimer : MonoBehaviour
{
    public enum Events { Neutral, Apocalypse, AddMissionSlot, ExtendMissionPool, ReduceMissionPool, EnlargeMissionPool};
    public List<Event> events;
    public float gameTimer;
    [HideInInspector] public float timer;
    public UnityEvent LevelEnd;
    public Slider timerSlider;
    public Transform eventFolder;
    public GameObject eventVisualPrefab;
    public Sprite apocalypseImage;
    public Sprite newPageImage;
    private Player[] players;
    MissionManager missionManager;
    private void Start()
    {
        missionManager = GetComponent<MissionManager>();
        SortList();
        RectTransform r = timerSlider.transform as RectTransform;
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
        GameTimerFunction();

        TimelineEvent();
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
            StartCoroutine(TileSystem.Instance.SinkWorld(players[0].tileUnder));
        }

        timerSlider.value = timer / gameTimer;
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
                default:
                    break;
            }
            events[0].eventTrigger?.Invoke();
            events.RemoveAt(0);
        }
    }
}
