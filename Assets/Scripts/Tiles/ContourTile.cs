using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContourTile : MonoBehaviour
{
    Transform myParent;
    Tile tile;
    MeshRenderer meshRenderer;
    private void Start()
    {
        tile = transform.parent.GetComponent<Tile>();
        myParent = transform.parent;
        meshRenderer = GetComponent<MeshRenderer>();
        transform.parent = null;
    }

    private void Update()
    {
        if(tile.walkable && !meshRenderer.enabled) 
        { 
            meshRenderer.enabled = true;
        }
        else if(!tile.walkable && meshRenderer.enabled)
        {
            meshRenderer.enabled = false;
        }
        if(myParent.position.y == myParent.position.y / tile.heightByTile && transform.position.y != myParent.position.y) 
        { 
            transform.position = myParent.position;
        }
    }
}
