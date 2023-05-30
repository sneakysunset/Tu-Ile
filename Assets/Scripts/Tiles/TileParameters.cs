using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileParameters : MonoBehaviour
{
    public float degradationTileTimerMax, degradationTileTimerMin;
    public AnimationCurve degradationTimerAnimCurve;
    public float heightByTile;
    public float shakeActivationTime = 30f;
    public float shakeFrequency = .03f;
    private void OnValidate()
    {
        TileSystem tileSystem = GetComponent<TileSystem>();
        tileSystem.UpdateParameters = true;
    }
}
