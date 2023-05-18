using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Scriptable Objects/Missions", menuName = "Missions/TileCreation", order = 1)]
public class SOM_Tile : SO_Mission
{
    public int requiredNumber;
    public TileType requiredType;

    public override void OnActivated(MissionUI mUIInGame, MissionUI mUIInPause, ref missionPage page)
    {
        
        base.OnActivated(mUIInGame, mUIInPause, ref page);
        switch (requiredType)
        {
            case TileType.Wood: 
                mUIInGame.missionChecker.sprite = ressourcesManager.mSTileCreation[0];
                mUIInPause.missionChecker.sprite = ressourcesManager.mSTileCreation[0];
                break;
            case TileType.Rock: 
                mUIInGame.missionChecker.sprite = ressourcesManager.mSTileCreation[1]; 
                mUIInPause.missionChecker.sprite = ressourcesManager.mSTileCreation[1]; 
                break;
            case TileType.Diamond:
                mUIInGame.missionChecker.sprite = ressourcesManager.mSTileCreation[2]; 
                mUIInPause.missionChecker.sprite = ressourcesManager.mSTileCreation[2]; 
                break;
            default: Debug.LogError("TYPE MISSMATCH"); break;
        }
        TileCounter tileCounter = FindObjectOfType<TileCounter>();
        page.numOfTileOnActivation = tileCounter.GetStat(requiredType);
        mUIInGame.missionText.text = (tileCounter.GetStat(requiredType) - page.numOfTileOnActivation).ToString() + " / " + requiredNumber.ToString();
        mUIInPause.missionText.text = description + " " + (tileCounter.GetStat(requiredType) - page.numOfTileOnActivation).ToString() + " / " + requiredNumber.ToString();
    }
}
