using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class TankSelectionManager : MonoBehaviour
    {
        public int PlayerIndex = 1;

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                PlayerIndex = 1;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                PlayerIndex = 2;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                PlayerIndex = 3;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                PlayerIndex = 4;
            }
        }
    }
}