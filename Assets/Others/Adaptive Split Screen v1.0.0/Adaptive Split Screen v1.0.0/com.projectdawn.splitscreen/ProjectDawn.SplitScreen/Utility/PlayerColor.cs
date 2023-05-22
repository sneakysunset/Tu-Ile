using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class PlayerColor
    {
        public readonly static Color32 PlayerA = new Color32(242, 99, 0, 160);
        public readonly static Color32 PlayerB = new Color32(0, 102, 181, 160);
        public readonly static Color32 PlayerC = new Color32(206, 124, 162, 160);
        public readonly static Color32 PlayerD = new Color32(255, 160, 10, 160);
        public readonly static Color32[] Players = new Color32[]
        {
            PlayerA,
            PlayerB,
            PlayerC,
            PlayerD
        };
    }
}
