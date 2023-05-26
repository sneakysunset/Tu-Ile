using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMovements : MonoBehaviour
{
    [Range(0,10)]public float degradationLerpSpeed;
    [Range(0,10)]public float tileGrowthLerpSpeed;
   

    private void TileMovement()
    {
        foreach (Tile tile in TileSystem.Instance.tiles)
        {
            Vector3 localPos = tile.transform.localPosition;
            float distance = 1;

            distance = Mathf.Abs(tile.transform.position.y - tile.currentPos.y);
            distance = Mathf.Clamp(distance, .3f, 5f);

            if(tile.readyToRoll && tile.IsMoving)
            {
                if (tile.isGrowing)
                {
                    tile.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), (1 / tileGrowthLerpSpeed) * Time.deltaTime * distance));
                }
                else
                {
                    tile.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), degradationLerpSpeed * Time.deltaTime * tile.degSpeed * TileSystem.Instance.lerpingSpeed * distance));
                }   
            }
        }
    }

    private void FixedUpdate()
    {
        if(TileSystem.Instance.ready) TileMovement();
    }
}
