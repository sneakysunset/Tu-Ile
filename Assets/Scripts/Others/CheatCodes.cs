using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;
using System.IO;
[System.Serializable]
public struct InputEvent
{
    public UnityEvent CheatCodeEvent;
    public KeyCode CheatKeyCode;
    public CheatCodes.InputType cheatCodesType;
}

public class CheatCodes : MonoBehaviour
{
    public enum InputType { DOWN,UP,STAY}
    public List<InputEvent> inputEvents;
    

    private void Start()
    {
        levelLoader.ClearOptions();
        string path = Application.streamingAssetsPath + "/LevelMaps";
        FileInfo[] fileInfo = new DirectoryInfo(path).GetFiles();

        List<string> options = new List<string>();
        foreach (FileInfo fi in fileInfo)
        {
            string[] d = fi.Name.Split('.');
            if(d.Length == 2) options.Add(d[0]);
        }

        levelLoader.AddOptions(options);
    }

    private void Update()
    {
        foreach(InputEvent input in inputEvents)
        {
            switch (input.cheatCodesType)
            {
                case CheatCodes.InputType.DOWN:
                    if (Input.GetKeyDown(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
                case CheatCodes.InputType.UP:
                    if (Input.GetKeyUp(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
                case CheatCodes.InputType.STAY:
                    if (Input.GetKey(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
            }
        }
    }

    public void LoadLevel() 
    {
        levelLoader.gameObject.SetActive(false);
        int selectedIndex = levelLoader.value;
        string selectedOption = levelLoader.options[selectedIndex].text;
      
        TileSystem.Instance.fileName = selectedOption.Split('_')[1];
        TileSystem.Instance.previousCenterTile = FindObjectOfType<Player>().tileUnder;
        bool toHub = false;
        if (selectedOption == "Hub") toHub = true;
        StartCoroutine(GridUtils.SinkWorld(FindObjectOfType<Player>().tileUnder, false, toHub));
    }

    [SerializeField] private TMP_Dropdown levelLoader;    
    public void ActivateLevelLoader()
    {
        if(levelLoader.gameObject.activeInHierarchy) levelLoader.gameObject.SetActive(false);
        else levelLoader.gameObject.SetActive(true);   
    }
}
