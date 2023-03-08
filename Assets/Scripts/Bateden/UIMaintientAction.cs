using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMaintientAction : MonoBehaviour
{
    public Image action;

    public void UpdateInfoAction(float tempsDAction, float actuelTemps)
    {
        action.fillAmount = actuelTemps / tempsDAction;
    }
    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position- Camera.main.transform.position);
    }
}
