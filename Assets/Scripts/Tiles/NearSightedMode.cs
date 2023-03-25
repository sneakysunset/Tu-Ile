using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearSightedMode : MonoBehaviour
{
    TileSystem tileS;
    GameObject[] players;
    public bool isNearSighted;

    public AnimationCurve distanceConvertorCurve;
    public float fallDistance;
    [Range(1, 100)]public float sightRange;
    private bool nsFlag;
    [Range(0,1)]public float lerpSpeed;
    [Range(0,1)]public float degradationLerpSpeed;
    [Range(0,1)]public float tileGrowthLerpSpeed;

    private void Start()
    {
        tileS = GetComponent<TileSystem>();
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void FixedUpdate()
    {
        if(isNearSighted)
        {


            foreach (Tile tile in tileS.tiles)
            {
                  Transform pl = null;
                  float dist = Mathf.Infinity;
                  foreach (GameObject p in players)
                  {
                        float tempDist = Vector2.Distance(new Vector3(p.transform.position.x, p.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z));
                        if(tempDist < dist)
                        {
                            dist = tempDist;
                            pl = p.transform;
                        }
                  }
                  float distance = dist / sightRange;
                  if (tile.isGrowing)
                  {
                      float evaluatedDistance = distanceConvertorCurve.Evaluate(distance);
                      tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y - evaluatedDistance * fallDistance, tile.transform.position.z), 100 * tileGrowthLerpSpeed * Time.deltaTime);
                  }
                  else
                  {
                      float evaluatedDistance = distanceConvertorCurve.Evaluate(distance);
                      tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y - evaluatedDistance * fallDistance, tile.transform.position.z), 100 * lerpSpeed * Time.deltaTime);
                  }
            }
                
            
            nsFlag = true;
        }
        else if(nsFlag)
        {
            nsFlag = false;
            foreach (Tile tile in tileS.tiles)
            {
/*                float distance = distanceConvertorCurve.Evaluate(Vector2.Distance(new Vector2(tileS.inputs.selectedTile.transform.position.x, tileS.inputs.selectedTile.transform.position.z), new Vector2(tile.transform.position.x, tile.transform.position.z)));
                tile.transform.position = tile.currentPos;*/

            }
        }

        if (!isNearSighted)
        {
            foreach (Tile tile in tileS.tiles)
            {
                if (tile.isGrowing)
                {
                    tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y, tile.transform.position.z), 100 * tileGrowthLerpSpeed * Time.deltaTime); 
                }
                else
                {
                    tile.transform.position = Vector3.MoveTowards(tile.transform.position, new Vector3(tile.transform.position.x, tile.currentPos.y, tile.transform.position.z), 100 * degradationLerpSpeed * Time.deltaTime); ;
                }
            }
        }
    }
}
