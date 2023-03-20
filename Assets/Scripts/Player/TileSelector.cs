using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class TileSelector : MonoBehaviour
{
    TileSystem tileS;
    Tile tileUnder;
    public Transform tileBluePrint;
    public float maxAngleToTarget = 50;
    public LayerMask tileLayer;
    public float hitDistance = 4;
    private Tile targettedTile;
    RessourcesManager rManager;
    private void Start()
    {
        tileS = FindObjectOfType<TileSystem>();
        rManager = FindObjectOfType<RessourcesManager>();
    }

    private void Update()
    {
        tileUnder = tileS.WorldPosToTile(transform.position);
        
        if (Physics.Raycast(tileUnder.transform.position, transform.forward, out RaycastHit hit, hitDistance, tileLayer) && hit.transform.TryGetComponent<Tile>(out targettedTile) && !targettedTile.walkable)
        {
            tileBluePrint.position = targettedTile.transform.position + Vector3.up * 25;
        }
        else
        {
            tileBluePrint.position = new Vector3(0, -100, 0);
            targettedTile = null; 
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


    public void OnSpawnTile(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(targettedTile != null && rManager.wood >= rManager.tileCost)
            {
                rManager.wood -= rManager.tileCost;
                targettedTile.Spawn(tileUnder.transform.position.y);
            }
        }
    }
}
