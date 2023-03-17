using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    TileSystem tileS;
    Tile tileUnder;
    public Transform tileBluePrint;
    public float maxAngleToTarget = 50;
    public LayerMask tileLayer;
    public float hitDistance = 4;
    private void Start()
    {
        tileS = FindObjectOfType<TileSystem>();
    }

    private void Update()
    {
        tileUnder = tileS.WorldPosToTile(transform.position);

        if (Physics.Raycast(tileUnder.transform.position, transform.forward, out RaycastHit hit, hitDistance, tileLayer) && TryGetComponent<Tile>(out Tile TargettedTile) && !TargettedTile.walkable)
        {
            tileBluePrint.position = TargettedTile.transform.position + Vector3.up * 20;
        }
        else
        {
            tileBluePrint.position = new Vector3(0, -100, 0);
        }
/*        float angle = 180;
        float tempAngle = 180;*/

/*        Tile targettedTile = null;
        foreach(Vector2Int vec in tileUnder.adjTCoords) 
        {
            tempAngle = Vector2.Angle(transform.forward, (tileS.tiles[vec.x, vec.y].transform.position - tileUnder.transform.position).normalized);
                Debug.DrawRay(transform.position, (tileS.tiles[vec.x, vec.y].transform.position - tileUnder.transform.position).normalized * 3, Color.blue) ;
            print(tempAngle);
            if(tempAngle < angle)
            {
                angle = tempAngle;
                targettedTile = tileS.tiles[vec.x, vec.y];
            }
        }
                Debug.DrawRay(transform.position, (targettedTile.transform.position - tileUnder.transform.position).normalized * 3, Color.red) ;
                Debug.DrawRay(transform.position, transform.forward.normalized * 3, Color.black) ;
        if(targettedTile != null && !targettedTile.walkable)
        {
            
        }
        else
        {
            
           // print(tileUnder.gameObject.name + " " + targettedTile.coordX + " " + targettedTile.coordY);
            
        }*/
        
    }

}
