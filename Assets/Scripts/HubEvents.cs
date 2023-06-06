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
    public Tile[] tile;
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
    public float beforeEventWaiter, afterEventWaiter;
    private void Start()
    {
        brains = TileSystem.Instance.cam.brains;
    }

    public void GrowTileList()
    {
        if(TileSystem.Instance.isHub && hubEventCoroutine == null && index < tileGrowEventList.Count && TileSystem.Instance.ready)
        {
            hubEventCoroutine = tileGrow(tileGrowEventList[index]);
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
        yield return new WaitForSeconds(beforeEventWaiter);
        string path = Application.streamingAssetsPath + "/LevelMaps/TM_Hub" + ".txt";
        for (int i = 0; i < tiles.tiles.Count; i++)
        {
            foreach(Tile tile in tiles.tiles[i].tile)
            {
                tile.Spawn(tiles.tiles[i].targetHeight, tiles.tiles[i].tileTypes.ToString());
                GridUtils.UpdateTileSave(GridUtils.GetStringByTile(tile), tile, path);
            }
            yield return new WaitForSeconds(timeOffsetBetweenTiles);
        }
        yield return new WaitForSeconds(afterEventWaiter);
        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.pM.canMove = true;
        }
        tiles.virtualCamera.Priority = 0;
        hubEventCoroutine = null;
    }   

    public void ResetGame()
    {
        index = 0;
        GridUtils.ResetGame();
    }
}
