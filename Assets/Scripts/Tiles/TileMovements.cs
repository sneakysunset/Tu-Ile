using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMovements : MonoBehaviour
{
    TileSystem tileS;
    [Range(0,10)]public float degradationLerpSpeed;
    [Range(0,10)]public float tileGrowthLerpSpeed;
    
    private void Start()
    {
        tileS = GetComponent<TileSystem>();
    }

    private void TileMovement()
    {
        foreach (Tile tile in tileS.tiles)
        {
            Vector3 localPos = tile.transform.localPosition;
            float distance = 1;

            distance = Mathf.Abs(tile.transform.position.y - tile.currentPos.y);
            distance = Mathf.Clamp(distance, 1f, 8f);

            if(tile.readyToRoll && tile.IsMoving)
            {
                if (tile.isGrowing)
                {
                    tile.transform.position = Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), (1 / tileGrowthLerpSpeed) * Time.deltaTime);
                }
                else
                {
                    tile.transform.position = Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), degradationLerpSpeed * Time.deltaTime * tile.degSpeed * tileS.lerpingSpeed * distance);
                }   
            }
        }
    }

    private void FixedUpdate()
    {
        TileMovement();
    }
}
