using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TriggerEffect : MonoBehaviour
{
    public UnityEvent OnTriggerEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
        {
            OnTriggerEvent?.Invoke();
        }
    }
}
