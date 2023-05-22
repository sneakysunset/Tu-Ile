using UnityEngine;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Draws property with min max slider <see cref="EditorGUI.MinMaxSlider"/>.
    /// </summary>
    public class MinMaxRange : PropertyAttribute
    {
        public float Min;
        public float Max;

        public MinMaxRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
