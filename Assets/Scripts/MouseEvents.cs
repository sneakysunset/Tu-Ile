using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEvents : MonoBehaviour
{
    Tile selectedTile;
    public LayerMask hitMask;
    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, hitMask, QueryTriggerInteraction.Ignore))
        {
            hit.transform.GetComponent<Tile>().OnSelected();
        }
    }
}
