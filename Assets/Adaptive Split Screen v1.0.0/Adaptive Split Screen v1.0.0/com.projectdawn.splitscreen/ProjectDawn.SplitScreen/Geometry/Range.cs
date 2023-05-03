using System;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public struct Range
    {
        public float From;
        public float To;

        public float MidPoint => (From + To) * 0.5f;
        public float Length => To - From;

        public Range(float from, float to)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (from > to)
                throw new ArgumentException("In range From value must be less or equal To value.");
#endif

            From = from;
            To = to;
        }

        /// <summary>
        /// Returns minimum distance between two ranges.
        /// </summary>
        public static float Distance(Range lhs, Range rhs)
        {
            var lhsMidPoint = lhs.MidPoint;
            var rhsMidPoint = rhs.MidPoint;

            var lhsRadius = lhs.Length * 0.5f;
            var rhsRadius = rhs.Length * 0.5f;

            var distance = math.abs(rhsMidPoint - lhsMidPoint);

            return math.max(0, distance - lhsRadius - rhsRadius);
        }
    }
}
