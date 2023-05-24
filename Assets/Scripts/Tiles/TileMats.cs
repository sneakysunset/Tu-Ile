using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileMats : MonoBehaviour
{
    public Mesh meshCollider;

    [Foldout("Center Tile")] public Mesh centerTileMesh;
    [Foldout("Center Tile")] public Material centerTileMat, centerTileMatBottom;
    [Foldout("Undegradable Tile")] public Mesh undegradableTileMesh;
    [Foldout("Undegradable Tile")] public Material undegradableTileMat, undegradableTileMatBottom;
    [Foldout("Plaine Tile")] public Mesh defaultTileMesh;
    [Foldout("Plaine Tile")] public Material plaineTileMatTop, plaineTileMatBottom;
    [Foldout("Wood Tile")] public Mesh woodTileMesh;
    [Foldout("Wood Tile")] public Material woodTileMat;
    [Foldout("Rock Tile")] public Mesh rockTileMesh;
    [Foldout("Rock Tile")] public Material rockTileMat;
    //[Foldout("Gold Tile")] 
    [Foldout("Gold Tile")] public Material goldTileMat;
    //[Foldout("Diamond Tile")]
    [Foldout("Diamond Tile")] public Material diamondTileMat;
    //[Foldout("Adamantium Tile")] public Mesh;
    [Foldout("Adamantium Tile")] public Material adamantiumTileMat;
    [Foldout("Sand Tile")] public Mesh sandTileMesh;
    [Foldout("Sand Tile")] public Material desertTileMatTop, desertTileMatBottom;
    //[Foldout("Bouncy Tile")]
    [Foldout("Bouncy Tile")] public Material bounceTileMat;
    [Foldout("Disabled Tile")] public Material disabledTileMaterial;
    [Foldout("Filons Prefabs")] public GameObject treePrefab, rockPrefab, goldPrefab, diamondPrefab, adamantiumPrefab;

    [Foldout("Colors")] public Color notWalkedOnColor, walkedOnColor, acceleratedDegradationColor;
    private void OnValidate()
    {
        TileSystem.Instance.UpdateParameters = true;
    }
}
