using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVBinding : MonoBehaviour
{
    Mesh m;

    private void Start()
    {
        /*m = GetComponent<MeshFilter>().mesh;
        List<int> uvListCoord = new List<int>();
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        m.uv5 = new Vector2[m.normals.Length];
        for (int i = 0; i < m.normals.Length; i++)
        {
            Vector3 world_v = localToWorld.MultiplyPoint3x4(m.vertices[i]);
            //print(world_v);
            if (world_v.y > 26)
            {
                var a = world_v - transform.position;
                a.y = 0;
                //print(a);
                uvListCoord.Add(i);
                m.uv5[i].x = 1; 
                m.uv5[i].y = 1; 
            }
            else
            {
                m.uv5[i] = new Vector2(-1, -1);
            }
        }*/
        
    }


}
