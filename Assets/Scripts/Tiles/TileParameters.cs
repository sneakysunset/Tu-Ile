using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileParameters : MonoBehaviour
{
    [Header("TerraForming")]
    [Space(15)]
    public float terraFormingSpeed = 10;
    [Range(0, 1)] public float terraFormingNormalisingSpeed = .1f;
    [Space(3)]
    [Header("At which height from it's original position will the tile atteign max speed when normalising")]
    public float distanceSpeedNormaliserModifier = 25;
    [Space(30)]
    [Header("Bump Variables")]
    [Space(15)]
    public float bumpStrength = 10;
    [Space(3)]
    [Header("The curve that translates the distance in y axis between the bumped item and the tile in bump strength")]
    public AnimationCurve bumpDistanceCurve;

    [Space(30)]
    [Header("Degradation")]
    [Space(15)]
    public float minTimer;
    public float maxTimer;
    public AnimationCurve degradationTimerAnimCurve;
    public float timeToGetToMaxDegradationSpeed;
    public float degradingSpeed;
    public float heightByTile;
}
