using System.Collections.Generic;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Comparer used to sort convext polygon points into counter clockwise.
    /// </summary>
    public struct PointWithCenterClockwiseSort : IComparer<float2>
    {
        public float2 Center;

        public PointWithCenterClockwiseSort(float2 center)
        {
            Center = center;
        }

        public int Compare(float2 a, float2 b)
        {
            float theta_a = ToAngle(a, Center);
            float theta_b = ToAngle(b, Center);
            return theta_a < theta_b ? -1 : theta_a > theta_b ? 1 : 0;
        }

        /// <summary>
        /// Return polar angle of p with respect to origin o
        /// </summary>
        float ToAngle(float2 p, float2 o) 
        {
            return math.atan2(p.y - o.y, p.x - o.x);
        }
    }
}
