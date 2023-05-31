using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable Objects/CrateReward", menuName = "Recettes/CrateReward", order = 1)]
public class SO_CrateReward : ScriptableObject
{
    public bool itemReward; 
    public bool isRandom;
    public int scoreReward;
    [HideIf("itemReward")] public rewardPStateStruct[] playerStateReward;
    [ShowIf("itemReward")] public List<rewardStruct> itemRewards;
}
