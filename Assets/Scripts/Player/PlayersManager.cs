using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayersManager : MonoBehaviour
{
    //public static int playerNumber = 1;
    [HideInInspector] public Player[] players;
    public static PlayersManager Instance;
    CameraCtr cam;

    private void OnApplicationQuit()
    {
        //playerNumber = 1;
        PlayerPrefs.SetInt("PlayerNumber", 1);
    }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("PlayerNumber") == 0) PlayerPrefs.SetInt("PlayerNumber", 1);
        players = FindObjectsOfType<Player>();
        cam = FindObjectOfType<CameraCtr>();
        PauseMenu pM = FindObjectOfType<PauseMenu>();
        pM.gameObject.SetActive(false);
        pM.transform.GetChild(0).gameObject.SetActive(true);
        SplitScreenEffect sse = FindObjectOfType<SplitScreenEffect>();
        for (int i = 0; i < players.Length; i++)
        {
            if(i > PlayerPrefs.GetInt("PlayerNumber") - 1)
            {
                players[i].gameObject.SetActive(false);
                foreach(ScreenData sd in sse.Screens)
                {
                    if(sd.Target == players[i].dummyTarget)
                    {
                        sse.Screens.Remove(sd);
                        break;
                    }
                }
                
            }
            else
            {
                cam.AddPlayer(players[i].dummyTarget);

                players[i].GetComponent<Player_Pause>().pauseMenu = pM;
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to the onDeviceChange event
        InputSystem.onDeviceChange += PlayerDisconnect;
    }

    private void OnDisable()
    {
        // Unsubscribe from the onDeviceChange event
        InputSystem.onDeviceChange -= PlayerDisconnect;
    }


    public void PlayerDisconnect(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected && TileSystem.Instance.isHub)
        {
            PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput>();
            foreach (PlayerInput playerInput in playerInputs)
            {
                InputUser user = playerInput.user;
                if (user != null && (user.pairedDevices.Contains(device) || user.lostDevices.Contains(device)))
                {
                    cam.RemovePlayer(playerInput.GetComponent<Player>().dummyTarget);
                    playerInput.gameObject.SetActive(false);
                    playerInput.DeactivateInput();
                    
                    break;
                }
            }
        }
    }
}
