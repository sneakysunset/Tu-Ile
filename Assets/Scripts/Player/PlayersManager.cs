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
    [HideInInspector] public Player[] players;
    public static PlayersManager Instance;
    private PlayerInputManager playerInputManager;
    CameraCtr cam;
    public GameObject pnc;
    [SerializeField] private PauseMenu pM, pHM;

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



    private IEnumerator Start()
    {
        cam = TileSystem.Instance.cam;
        players = FindObjectsOfType<Player>();
        playerInputManager = GetComponent<PlayerInputManager>();
        //playerInputManager.DisableJoining();
        //pM.transform.GetChild(0).gameObject.SetActive(true);
        //pM.gameObject.SetActive(false);
        //SplitScreenEffect sse = FindObjectOfType<SplitScreenEffect>();
        /*for (int i = 0; i < players.Length; i++)
        {
            if(cam.players == null || cam.players.Count == 0)
            {
                cam.AddPlayer(players[0].dummyTarget, players[0]);      
            } 
            if (*//*(TileSystem.Instance.isHub) && *//*!cam.players.Contains(players[i].dummyTarget)) cam.AddPlayer(players[i].dummyTarget, players[i]);
            Player_Pause pPause = players[i].GetComponent<Player_Pause>();
            print(pHM);
            pPause.pauseHubMenu = pHM;
            pPause.pauseMenu = pM;
            //print(pPause.pauseHubMenu);
        }*/

        //if(!cam.GetComponentInChildren<CinemachineBrain>().IsBlending) playerInputManager.EnableJoining();
        yield return new WaitUntil(() => TileSystem.Instance.ready);
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += PlayerDisconnect;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= PlayerDisconnect;
    }


    public void PlayerDisconnect(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected /*&& TileSystem.Instance.isHub*/)
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
