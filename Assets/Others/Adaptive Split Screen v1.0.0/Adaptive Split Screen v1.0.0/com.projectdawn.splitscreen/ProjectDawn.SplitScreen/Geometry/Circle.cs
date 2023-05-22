using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public struct Circle
    {
        public float2 Center;
        public float Radius;

        public Circle(float2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Returns minimum distance between two circles.
        /// </summary>
        public static float GetDistance(in Circle lhs, in Circle rhs)
        {
            // TODO: use distancesq
            return math.max(math.distance(lhs.Center, rhs.Center) - lhs.Radius - rhs.Radius, 0);
        }
    }
}
