using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public enum SpeedMode
    {
        VerySlow,
        Slow,
        Medium,
        Fast,
        VeryFast,
    }

    /// <summary>
    /// Makes player movement more smooth.
    /// </summary>
    public class SmoothModifier : MonoBehaviour, ISplitScreenTargetPosition, ISplitScreenTranslatingPosition
    {
        public SpeedMode Speed = SpeedMode.Medium;
        public bool Translating = true;

        float3[] m_PreviousPositions;

        void OnEnable()
        {
            if (TryGetComponent(out SplitScreenEffect splitScreen))
            {
                m_PreviousPositions = new float3[splitScreen.Screens.Count];
                for (int i = 0; i < splitScreen.Screens.Count; ++i)
                {
                    m_PreviousPositions[i] = (float3) splitScreen.Screens[i].Target.position;
                }
            }
        }

        public float3 OnSplitScreenTranslatingPosition(int index, float3 position)
        {
            if (!Application.isPlaying)
                return position;

            if (Translating)
                return position;

            return m_PreviousPositions[index];
        }

        public float3 OnSplitScreenTargetPosition(int index, float3 position)
        {
            if (!Application.isPlaying)
                return position;

            if (m_PreviousPositions == null)
                m_PreviousPositions = new float3[index + 1];

            if (m_PreviousPositions.Length <= index)
            {
                System.Array.Resize(ref m_PreviousPositions, index + 1);
            }

            var cameraPosition = m_PreviousPositions[index];
            var offset = (position - cameraPosition) * math.saturate(GetSpeed(Speed));
            cameraPosition += offset;

            m_PreviousPositions[index] = cameraPosition;

            return cameraPosition;
        }

        public float GetSpeed(SpeedMode mode)
        {
            float scale = 60f / (1f/Time.deltaTime);
            switch (mode)
            {
                case SpeedMode.VerySlow:
                    return scale * 0.01f;
                case SpeedMode.Slow:
                    return scale * 0.05f;
                case SpeedMode.Medium:
                    return scale * 0.1f;
                case SpeedMode.Fast:
                    return scale * 0.25f;
                case SpeedMode.VeryFast:
                    return scale * 0.5f;
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}
