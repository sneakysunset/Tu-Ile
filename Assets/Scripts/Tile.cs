using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isSelected;
    private MeshRenderer meshR;
    private void Start()
    {
        meshR = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        //meshR.material.color = Color.white;
    }

    public Vector3 indexToWorldPos(int x, int z, Vector3 ogPos)
    {
        float xOffset = 0;
        if (z % 2 == 1) xOffset = transform.localScale.x * .9f;
        Vector3 pos = ogPos + new Vector3(x * transform.localScale.x * 1.8f + xOffset, 0, z * transform.localScale.x * 1.5f);
        return pos;
    }

    public void OnSelected()
    {
        isSelected = true;
        meshR.material.color = Color.yellow;
    }

    private void LateUpdate()
    {
        if (isSelected)
        {
            isSelected = false;
        }
    }
}
