using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour
{
    public static event Action<GameObject, GameObject> OnSelectionUpdated;
    private static void DispatchSelectionUpdated(GameObject newSelectedGameObject, GameObject previousSelectedGameObject)
    {
        if (OnSelectionUpdated != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Ui/Button Switch");
            OnSelectionUpdated.Invoke(newSelectedGameObject, previousSelectedGameObject);
        }
    }

    [SerializeField]
    private EventSystem eventSystem;

    private GameObject m_LastSelectedGameObject;

    private void Update()
    {
        var currentSelectedGameObject = eventSystem.currentSelectedGameObject;
        if (currentSelectedGameObject != m_LastSelectedGameObject)
        {
            DispatchSelectionUpdated(currentSelectedGameObject, m_LastSelectedGameObject);
            m_LastSelectedGameObject = currentSelectedGameObject;
        }
    }
}

