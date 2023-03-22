using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailManager: MonoBehaviour
{
    private TrailRenderer trail;
    private float duration;
    private float timeStamp;

        private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }
    private void Update()
    {
        if (Time.time > timeStamp + duration)
        {
            duration = Random.Range(0.05f, 0.3f);
            timeStamp = Time.time;
            trail.emitting = !trail.emitting;
        }
    }
}
