using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Behaviour : MonoBehaviour
{
    TileSystem tileS;
    Player[] players;
    [HideInInspector] public List<Tile> tilePath;
    [HideInInspector] public Tile tileUnder;
    WaitForSeconds waiter;
    [Range(.1f, 20)] public float refreshRate = 1;
    public bool stopRefreshing;
    private void Start()
    {
        players = FindObjectsOfType<Player>();
        tileS = FindObjectOfType<TileSystem>();
        waiter = new WaitForSeconds(refreshRate);
        StartCoroutine(RefreshAIPath());
    }

    IEnumerator RefreshAIPath()
    {
        yield return new WaitUntil(()=> stopRefreshing = true);
        yield return waiter;
        InitializePathFinding();
        StartCoroutine(RefreshAIPath());
    }

    private void Update()
    {
        tileUnder = tileS.WorldPosToTile(transform.position);
    }

    public void InitializePathFinding()
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
        Tile targetTile = tileS.WorldPosToTile(target);

        tilePath = StepAssignment.Initialisation(targetTile, tileS, transform.position);
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
        stopRefreshing = true;
        tilePath.Clear();
    }
}
