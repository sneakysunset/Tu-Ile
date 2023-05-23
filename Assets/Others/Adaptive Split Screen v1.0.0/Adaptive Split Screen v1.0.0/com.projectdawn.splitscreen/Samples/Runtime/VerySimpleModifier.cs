using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectDawn.SplitScreen;
using Unity.Mathematics;

public class VerySimpleModifier : MonoBehaviour, ISplitScreenTargetPosition
{
    public int PlayerIndex;
    public float3 Offset;

    void OnEnable()
    {
        
    }

    public float3 OnSplitScreenTargetPosition(int screenIndex, float3 positionWS)
    {
        if (PlayerIndex != screenIndex)
            return positionWS;
        return positionWS + Offset;
    }
}
