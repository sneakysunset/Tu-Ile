using System;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Voronoi region of <see cref="VoronoiDiagram"/>.
    /// </summary>
    public struct VoronoiRegion : IEquatable<VoronoiRegion>
    {
        public int Index;
        internal int Offset;
        internal int Length;
        internal float2 Centroid;
        internal float Area;

        public static VoronoiRegion Null => new VoronoiRegion { Index = -1 };

        public bool Equals(VoronoiRegion other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object compare)
        {
            return compare is VoronoiRegion polygon && Equals(polygon);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(VoronoiRegion lhs, VoronoiRegion rhs)
        {
            return lhs.Index == rhs.Index;
        }

        public static bool operator !=(VoronoiRegion lhs, VoronoiRegion rhs)
        {
            return lhs.Index != rhs.Index;
        }
    }
}
