using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearSightedMode : MonoBehaviour
{
    TileSystem tileS;
    Transform player;
    public bool isNearSighted;
    public enum NearSightMode { Player, Tile}
    public NearSightMode nearSightMode;
    public AnimationCurve distanceConvertorCurve;
    public float fallDistance;
    [Range(1, 100)]public float sightRange;
    private bool nsFlag;
    [Range(0,1)]public float lerpSpeed;
    private void Start()
    {
        tileS = GetComponent<TileSystem>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if(nearSightMode == NearSightMode.Player && isNearSighted)
        {
            foreach (Tile tile in tileS.tiles)
            {
                float distance = (Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z))) / sightRange;
                float evaluatedDistance = distanceConvertorCurve.Evaluate(distance);
                tile.rb.MovePosition(Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y - evaluatedDistance * fallDistance, tile.transform.position.z), 100 * lerpSpeed * Time.deltaTime)) ;
            }
            nsFlag = true;
        }
        else if(nearSightMode == NearSightMode.Tile && isNearSighted)
        {
            foreach(Tile tile in tileS.tiles)
            {
                float distance = (Vector2.Distance(new Vector2(tileS.inputs.selectedTile.transform.position.x, tileS.inputs.selectedTile.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z)))/ (sightRange * 2);
                float evaluatedDistance = distanceConvertorCurve.Evaluate(distance);
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y - evaluatedDistance * fallDistance, tile.transform.position.z), 100 * lerpSpeed * Time.deltaTime);
            }
            nsFlag = true;
        }
        else if(nsFlag)
        {
            nsFlag = false;
            foreach (Tile tile in tileS.tiles)
            {
                float distance = distanceConvertorCurve.Evaluate(Vector2.Distance(new Vector2(tileS.inputs.selectedTile.transform.position.x, tileS.inputs.selectedTile.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z)));
                tile.transform.position = tile.currentPos;
            }
        }

        if (!isNearSighted)
        {
            foreach (Tile tile in tileS.tiles)
            {
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y, tile.transform.position.z), 100 * lerpSpeed * Time.deltaTime); ;
            }
        }
    }
}
