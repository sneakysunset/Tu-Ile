using UnityEngine;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Forces value not to be lower than specified.
    /// </summary>
    public class MaxValue : PropertyAttribute
    {
        public float Value;

        public MaxValue(float value)
        {
            Value = value;
        }
    }
}
