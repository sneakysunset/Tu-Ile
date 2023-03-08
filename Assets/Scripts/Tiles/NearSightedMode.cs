using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearSightedMode : MonoBehaviour
{
    TileSystem tileS;
    Transform player;
    public bool isNearSighted;
    public AnimationCurve distanceConvertorCurve;
    public float fallDistance;
    public float maxDistanceToFall;
    public enum NearSightMode { Player, Tile}
    public NearSightMode nearSightMode;
    private bool nsFlag;
    private void Start()
    {
        tileS = GetComponent<TileSystem>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if(nearSightMode == NearSightMode.Player && isNearSighted)
        {

        }
        else if(nearSightMode == NearSightMode.Tile && isNearSighted)
        {
            foreach(Tile tile in tileS.tiles)
            {
                float distance = (Vector2.Distance(new Vector2(tileS.inputs.selectedTile.transform.position.x, tileS.inputs.selectedTile.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z)))/ maxDistanceToFall;
                float evaluatedDistance = distanceConvertorCurve.Evaluate(distance);
                print(distance+ "  " + evaluatedDistance);
                tile.transform.position = new Vector3(tile.transform.position.x, tile.ogPos.y - evaluatedDistance * fallDistance, tile.transform.position.z);
            }
            nsFlag = true;
        }
        else if(nsFlag)
        {
            nsFlag = false;
            foreach (Tile tile in tileS.tiles)
            {
                float distance = distanceConvertorCurve.Evaluate(Vector2.Distance(new Vector2(tileS.inputs.selectedTile.transform.position.x, tileS.inputs.selectedTile.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z)));
                tile.transform.position = tile.ogPos;
            }
        }
    }
}
