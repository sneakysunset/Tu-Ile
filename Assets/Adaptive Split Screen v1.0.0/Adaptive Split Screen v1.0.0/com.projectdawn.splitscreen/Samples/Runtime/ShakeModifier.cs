using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public enum ShakeMode
    {
        Random,
        PerlinNoise,
    }

    public class ShakeModifier : MonoBehaviour, ISplitScreenTargetPosition
    {
        public ShakeMode Mode;
        public float MaxOffset = 1;
        [Range(0, 1)]
        public float Trauma = 0;
        public float Duration = 1.0f;

        public float3 OnSplitScreenTargetPosition(int index, float3 position)
        {
            if (!Application.isPlaying)
                return position;

            if (Trauma == 0)
                return position;

            float2 noise = float2.zero;
            if (Mode == ShakeMode.Random)
            {
                noise = new float2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));
            }
            else if (Mode == ShakeMode.PerlinNoise)
            {
                noise = new float2(Mathf.PerlinNoise(Time.time * 20, 0), Mathf.PerlinNoise(0, Time.time * 20));
                noise = noise * 2 - 1;
            }

            float shake = Trauma * Trauma;
            float2 offset = MaxOffset * shake * noise;
            position += new float3(offset, 0);

            return position;
        }

        void Update()
        {
            if (Duration == 0)
                return;
            Trauma = math.max(Trauma - Time.deltaTime * Duration, 0);
        }
    }
}
