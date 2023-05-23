using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileParameters : MonoBehaviour
{
    public float degradationTileTimer;
    public AnimationCurve degradationTimerAnimCurve;
    public float heightByTile;

    private void OnValidate()
    {
        TileSystem tileSystem = GetComponent<TileSystem>();
        tileSystem.UpdateParameters = true;
    }
}
