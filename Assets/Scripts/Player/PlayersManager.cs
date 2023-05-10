using Cinemachine;
using ProjectDawn.SplitScreen;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    public int playerNumber;
    [HideInInspector] public Player[] players;
    public static PlayersManager Instance;
    
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
        players = FindObjectsOfType<Player>();
        PauseMenu pM = FindObjectOfType<PauseMenu>();
        CameraCtr cam = FindObjectOfType<CameraCtr>();
        pM.gameObject.SetActive(false);
        pM.transform.GetChild(0).gameObject.SetActive(true);
        SplitScreenEffect sse = FindObjectOfType<SplitScreenEffect>();
        CinemachineTargetGroup targetGroupe = FindObjectOfType<CinemachineTargetGroup>();
        List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();
        for (int i = 0; i < players.Length; i++)
        {
            if(i > playerNumber - 1)
            {
                players[i].gameObject.SetActive(false);
                foreach(ScreenData sd in sse.Screens)
                {
                    if(sd.Target == players[i].transform)
                    {
                        sse.Screens.Remove(sd);
                        break;
                    }
                }
                
            }
            else
            {
                CinemachineTargetGroup.Target t = new CinemachineTargetGroup.Target();
                t.weight = 1;
                t.target = players[i].transform;
                targets.Add(t);
                players[i].GetComponent<Player_Pause>().pauseMenu = pM;
                cam.AddPlayer(players[i].transform);
            }
        }
        targetGroupe.m_Targets = new CinemachineTargetGroup.Target[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            targetGroupe.m_Targets[i] = targets[i]; 
        }


    }
}
