using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileMats : MonoBehaviour
{
    [Space(10)]
    [Header ("Editor Mats")]
    public Material disabledTileMaterial;
    public Material undegradableTileMat, sandTileMat, bounceTileMat;

    [Space(10)]
    [Header("Game Mats")]
    public Material plaineTileMat; 
    public Material woodTileMat, rockTileMat, goldTileMat, diamondTileMat, adamantiumTileMat;
    public Mesh defaultTileMesh, woodTileMesh, rockTileMesh;
    public GameObject treePrefab, rockPrefab, goldPrefab, diamondPrefab, adamantiumPrefab;
    private TileSystem tileSystem;

    [Space(10)]
    [Header("Colors")]
    public Color notWalkedOnColor;
    public Color walkedOnColor;
    public Color acceleratedDegradationColor;
    private void OnValidate()
    {
        tileSystem = GetComponent<TileSystem>();
        tileSystem.UpdateParameters = true;
    }
}
