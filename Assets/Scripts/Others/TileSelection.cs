using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



public class TileSelection : MonoBehaviour

{
#if UNITY_EDITOR
    public LayerMask tileLayer;
    private void OnDrawGizmosSelected()
    {
        //Vector3 pos = HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition).GetPoint(.1f);
        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition), out RaycastHit hit, tileLayer))
        {
            Tile tile = GridUtils.WorldPosToTile(hit.point);
            //Selection.activeObject = tile;
            if (!Selection.objects.Contains(tile))
            {
                Object[] objects = new Object[Selection.objects.Length];
                for (int i = 0; i < objects.Length ; i++)
                {
                   if (Selection.objects[i] as GameObject == gameObject) objects[i] = tile.gameObject;
                   else objects[i] = Selection.objects[i];
                }

                Selection.objects = objects;
            }

        }
    }
#endif
}
