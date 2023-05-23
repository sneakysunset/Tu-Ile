using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Item_Boussole : Item
{
    /*[HideInInspector] */public List<Tile> targettedTiles;
    private List<Transform> pointers;
    
    private void Start()
    {
        MissionManager mM = FindObjectOfType<MissionManager>();
        pointers = new List<Transform>();

        foreach(BoussoleMission bM in CompassMissionManager.Instance.activeM)
        {
            if (!targettedTiles.Contains(bM.targettedTile))
            {
                targettedTiles.Add(bM.targettedTile);
            }
        }

        foreach(missionPage page in mM.activeMissions) 
        { 
            if(page.boussoleTile != null && !targettedTiles.Contains(page.boussoleTile))
            {
                targettedTiles.Add(page.boussoleTile);
            }
        }
    }

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
        if (targettedTiles != null)
        {
            for(int i = 0; i < targettedTiles.Count; i++)
            {
                player.pointers[i].gameObject.SetActive(true);
                pointers.Add(player.pointers[i]);
            }
        }
    }

    public void UpdateTargettedList()
    {
        if(isHeld && targettedTiles != null)
        {
            foreach(Transform pointer in pointers)
            {
                pointer.gameObject.SetActive(false);
            }
            pointers.Clear();

            for (int i = 0; i < targettedTiles.Count; i++)
            {
                _player.pointers[i].gameObject.SetActive(true);
                pointers.Add(_player.pointers[i]);
            }
        }
    }

    public override void GrabRelease(bool etablied)
    {
        base.GrabRelease(etablied);
        foreach (Transform pointer in pointers)
        {
            pointer.gameObject.SetActive(false);
        }
        pointers.Clear();
    }

    public override void Update()
    {
        base.Update();

        if (targettedTiles != null && isHeld)
        {
            for (int i = 0; i < pointers.Count; i++)
            {
                pointers[i].LookAt(new Vector3(targettedTiles[i].transform.position.x, pointers[i].position.y, targettedTiles[i].transform.position.z));
            }
        }
    }

    public override void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        base.ThrowAction(player, throwStrength, direction);
        foreach (Transform pointer in pointers)
        {
            pointer.gameObject.SetActive(false);
        }
        pointers.Clear();
    }

    private void OnDestroy()
    {
        foreach (Transform pointer in pointers)
        {
            pointer.gameObject.SetActive(false);
        }
        pointers.Clear();
    }
}
