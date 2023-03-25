using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BirdAnimBake : MonoBehaviour
{
    private ParticleSystemRenderer particles;

    public SkinnedMeshRenderer renderr;

    Mesh m;

    void Start()
    {
        m = new Mesh();
        particles = GetComponent<ParticleSystemRenderer>();
    }

    void LateUpdate()
    {
        renderr.BakeMesh(m);

       
        particles.mesh = m;
    }
}
