using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEvent : MonoBehaviour
{
    TileSystem tileS;
    public float heightDiffToStopWave;
    private InputEvents inputEvents;
    private WaitForSeconds waiter;
    void Start()
    {
        waiter = new WaitForSeconds(.2f);
        inputEvents = GetComponent<InputEvents>();
        tileS = FindObjectOfType<TileSystem>();
    }


    public void Instigator()
    {
        Tile firstTile = inputEvents.selectedTile;
        
        int x = firstTile.coordX;
        int y = firstTile.coordY;
        int offset = 1;
        if (y % 2 == 0) offset = -1;
        StartCoroutine(VoidEffect(x + offset, y + 1, x, y));
        //StartCoroutine(VoidEffect(x + 0, y + 1, x, y));
        //StartCoroutine(VoidEffect(x - 1, y + 0, x, y));
        //StartCoroutine(VoidEffect(x + 1, y + 0, x, y));
        //StartCoroutine(VoidEffect(x + 0, y - 1, x, y));
        //StartCoroutine(VoidEffect(x + offset, y - 1, x, y));
    }

    IEnumerator VoidEffect(int hitX, int hitY, int fromX, int fromY)
    {
        yield return waiter;
        if (hitX < 0 || hitX > tileS.rows || hitY < 0 || hitY > tileS.columns ||
        !tileS.tiles[hitX, hitY].walkable ||
        tileS.tiles[hitX, hitY].transform.position.y - tileS.tiles[fromX, fromY].transform.position.y > heightDiffToStopWave)
        { }
        else
        {
            tileS.tiles[hitX, hitY].currentPos += 3 * Vector3.up;
            int offset = 0;
            if (hitY % 2 == 0) offset = 0;
            int x = hitX - fromX;
            int y = hitY - fromY;
            StartCoroutine(VoidEffect(x, y, hitX, hitY));
        }
    }
}
