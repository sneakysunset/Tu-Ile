using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public struct tileList
{
    public List<tileStruct> tiles;
    public CinemachineVirtualCamera virtualCamera;

}

[System.Serializable]
public struct tileStruct
{
    public Tile tile;
    public int targetHeight;
    public Item.StackType tileTypes;
}

public class HubEvents : MonoBehaviour
{
    public List<tileList> tileGrowEventList;
    private CinemachineBrain[] brains;
    [HideInInspector] public int index = 0;
    public float timeOffsetBetweenTiles;
    IEnumerator hubEventCoroutine;
    private void Start()
    {
        brains = TileSystem.Instance.cam.brains;
    }

    public void GrowTileList()
    {
        if(TileSystem.Instance.isHub && hubEventCoroutine == null && index < tileGrowEventList.Count && TileSystem.Instance.ready)
        {
            hubEventCoroutine = tileGrow(tileGrowEventList[0]);
            index++;
            StartCoroutine(hubEventCoroutine);
        }
    }

    IEnumerator tileGrow(tileList tiles)
    {
        tiles.virtualCamera.Priority = 10;
        foreach(Player player in FindObjectsOfType<Player>())
        {
            player.pM.canMove = false;
            
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(()=> !brains[0].IsBlending);
        yield return new WaitForSeconds(1);
        string path = Application.streamingAssetsPath + "/LevelMaps/TM_Hub" + ".txt";
        for (int i = 0; i < tiles.tiles.Count; i++)
        {
            tiles.tiles[i].tile.Spawn(tiles.tiles[i].targetHeight, tiles.tiles[i].tileTypes.ToString(), 1);
            GridUtils.UpdateTileSave(GridUtils.GetStringByTile(tiles.tiles[i].tile), tiles.tiles[i].tile, path);
            yield return new WaitForSeconds(timeOffsetBetweenTiles);
        }
        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.pM.canMove = true;
        }
        tiles.virtualCamera.Priority = 0;
        tileGrowEventList.RemoveAt(0);
        hubEventCoroutine = null;
    }

    public void ResetGame()
    {
        index = 0;
        GridUtils.ResetGame();
    }
}
