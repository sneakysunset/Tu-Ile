using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileSelection : MonoBehaviour
{
    public LayerMask tileLayer;
    private void OnDrawGizmosSelected()
    {
        //Vector3 pos = HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition).GetPoint(.1f);
        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition), out RaycastHit hit, tileLayer))
        {
            Tile tile = GridUtils.WorldPosToTile(hit.point);
            Selection.activeObject = tile;

        }
    }
}
