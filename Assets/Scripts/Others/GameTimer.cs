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
    public enum Events { Apocalypse};
    public List<Event> events;
    public float gameTimer;
    [HideInInspector] public float timer;
    public UnityEvent LevelEnd;
    public Slider timerSlider;
    public Transform eventFolder;
    public GameObject eventVisualPrefab;
    public Sprite apocalypseImage;
    private void Start()
    {
        SortList();
        RectTransform r = timerSlider.transform as RectTransform;
        foreach(Event e in events)
        {
            RectTransform t = Instantiate(eventVisualPrefab, eventFolder).transform as RectTransform;
            t.localPosition += new Vector3((e.eventTime / gameTimer) * 2 * (r.sizeDelta.x * r.localScale.x), 0, 0);
            //t.GetComponent<Image>().sprite = apocalypseImage;
        }
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
        }

        timerSlider.value = timer / gameTimer;

/*        int minutes = Mathf.FloorToInt((gameTimer - timer) / 60);

        if (minutes >= 1)
        {
            timerText.text = minutes.ToString() + " : " + Mathf.RoundToInt((gameTimer - timer) % 60).ToString();
        }
        else
        {
            timerText.text = Mathf.RoundToInt((gameTimer - timer)).ToString();
        }*/
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
                default:
                    break;
            }
            events[0].eventTrigger?.Invoke();
            events.RemoveAt(0);
        }
    }
}
