using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AI_Behaviour : MonoBehaviour
{
    public enum AITarget { ClosestPlayer, RandomTileAround};
    public AITarget target;
    TileSystem tileS;
    Player[] players;
    [HideInInspector] public List<Tile> tilePath;
    [HideInInspector] public Tile tileUnder;
    [Range(.1f, 20)] public float refreshRateMin = 1, refreshRateMax = 2;
    [Header("numTilesAround only useful for RandomTileAround AITarget")]
    public int numTilesAround;
    public enum Behavious { AI, Target, Static}
    public Behavious currentBehavious;
    [HideInInspector] public Tile targetTile;
    private void Start()
    {
        players = FindObjectsOfType<Player>();
        tileS = FindObjectOfType<TileSystem>();
        StartCoroutine(RefreshAIPath());
    }

    IEnumerator RefreshAIPath()
    {
        yield return new WaitUntil(()=>  currentBehavious == Behavious.AI);
        yield return new WaitUntil(()=> tilePath.Count == 0);
        float refreshRate = Random.Range(refreshRateMin, refreshRateMax);
        yield return new WaitForSeconds(refreshRate);

        switch (target)
        {
            case AITarget.ClosestPlayer:
                targetTile = GetClosestPlayer();
                break;
            case AITarget.RandomTileAround:
                List<Tile> list = GridUtils.GetTilesAround(numTilesAround, tileUnder);
                targetTile = list[Random.Range(0, list.Count - 1)];
                break;
        }

        InitializePathFinding(targetTile);
        StartCoroutine(RefreshAIPath());
    }

    private void Update()
    {
        tileUnder = GridUtils.WorldPosToTile(transform.position);
    }

    private Tile GetClosestPlayer()
    {
        float distance = Mathf.Infinity;
        Vector3 target = Vector3.zero;
        foreach (Player p in players)
        {
            float tempDistance = Vector3.Distance(p.transform.position, transform.position);
            if (tempDistance < distance)
            {
                distance = tempDistance;
                target = p.transform.position;
            }
        }
        Tile targetTile = GridUtils.WorldPosToTile(target);
        return targetTile;
    }

    public void InitializePathFinding(Tile targetTile)
    {
       

        tilePath = StepAssignment.Initialisation(targetTile, tileS, tileUnder);
        Vector3 pos = transform.position;
        for (int i = 0; i < tilePath.Count; i++)
        {
            Vector3 previousPos = pos;
            pos = tilePath[i].transform.position + 23 * Vector3.up;
            Debug.DrawLine(previousPos, pos, Color.black, 100);
        }
    }

    public void ClearPath()
    {
        currentBehavious = Behavious.Static;
        tilePath.Clear();
    }

}
