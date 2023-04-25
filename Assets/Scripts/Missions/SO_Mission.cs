using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SO_Mission : ScriptableObject
{
    public string description;
    public float deliveryTime;
    public virtual void OnActivated(Image _missionChecker, TextMeshProUGUI _missionText, ref missionPage page)
    {
        page.timer = deliveryTime;
        page.activated = true;
        _missionChecker.color = Color.black;
        _missionText.color = Color.white;
    }

    public virtual void OnCompleted(ref missionPage page)
    {
        page.activated = false;
    }
}
