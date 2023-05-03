using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    [RequireComponent(typeof(SplitScreenEffect))]
    public class PlayerController : MonoBehaviour
    {
        public bool Is3D = false;
        public float Speed = 1;
        int PlayerIndex;

        void Update()
        {
            var splitScreen = GetComponent<SplitScreenEffect>();

            if (Input.GetKeyUp(KeyCode.Alpha1) && splitScreen.Screens.Count >= 1)
            {
                PlayerIndex = 0;
            }
            if (Input.GetKeyUp(KeyCode.Alpha2) && splitScreen.Screens.Count >= 2)
            {
                PlayerIndex = 1;
            }
            if (Input.GetKeyUp(KeyCode.Alpha3) && splitScreen.Screens.Count >= 3)
            {
                PlayerIndex = 2;
            }
            if (Input.GetKeyUp(KeyCode.Alpha4) && splitScreen.Screens.Count >= 4)
            {
                PlayerIndex = 3;
            }

            if (splitScreen.Screens.Count == 0)
                return;

            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            float3 vector = Is3D ? new float3(horizontal, 0, vertical) : new float3(horizontal, vertical, 0);
            float intensity = math.length(vector);
            float3 force = math.normalizesafe(vector) * intensity * Time.deltaTime * Speed;
            splitScreen.Screens[PlayerIndex].Target.position += (Vector3)force;
        }
    }
}
