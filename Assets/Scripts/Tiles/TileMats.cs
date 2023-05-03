using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileMats : MonoBehaviour
{
    [Header ("Editor Mats")]
    public Material disabledTileMaterial;
    public Material undegradableTileMat, sandTileMat, bounceTileMat;
    [Header("Game Mats")]
    public Material plaineTileMat; 
    public Material woodTileMat, rockTileMat, goldTileMat, diamondTileMat, adamantiumTileMat;
    public Mesh defaultTileMesh, woodTileMesh, rockTileMesh;
    public GameObject treePrefab, rockPrefab, goldPrefab, diamondPrefab, adamantiumPrefab;
    private TileSystem tileSystem;

    private void OnValidate()
    {
        tileSystem = GetComponent<TileSystem>();
        tileSystem.UpdateParameters = true;
    }
}
