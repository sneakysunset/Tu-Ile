using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponentReferencer : MonoBehaviour
{
    #region Parameters
    [Foldout("Parameters")] public float tourbillonSpeed;
    #endregion
    #region Mesh & Mat
    public Mesh meshCollider;
    [Foldout("Center Tile")] public Mesh centerMesh;
    [Foldout("Center Tile")] public Material centerMat, centerMatBottom;
    [Foldout("Center Tile")] public Material fcenterMat, fcenterMatBottom;
    [Foldout("Undegradable Tile")] public Mesh undegradableMesh;
    [Foldout("Undegradable Tile")] public Material undegradableMat, undegradableMatBottom;
    [Foldout("Undegradable Tile")] public Material fundegradableMat, fundegradableMatBottom;
    [Foldout("Plaine Tile")] public Mesh defaultMesh;
    [Foldout("Plaine Tile")] public Material plaineMatTop, plaineMatBottom;
    [Foldout("Plaine Tile")] public Material fplaineMatTop, fplaineMatBottom;
    [Foldout("Wood Tile")] public Mesh woodMesh;
    [Foldout("Wood Tile")] public Material woodMat;
    [Foldout("Wood Tile")] public Material fwoodMat;
    [Foldout("Rock Tile")] public Mesh rockMesh;
    [Foldout("Rock Tile")] public Material rockMat;
    [Foldout("Rock Tile")] public Material frockMat;
    [Foldout("Gold Tile")] public Material goldMat;
    [Foldout("Diamond Tile")] public Material diamondMat;
    [Foldout("Adamantium Tile")] public Material adamantiumMat;
    [Foldout("Sand Tile")] public Mesh sandMesh;
    [Foldout("Sand Tile")] public Material desertMatTop, desertMatBottom;
    [Foldout("Sand Tile")] public Material fdesertMatTop, fdesertMatBottom;
    [Foldout("Bouncy Tile")] public Material bounceMat;
    [Foldout("Bouncy Tile")] public Material fbounceMat;
    [Foldout("Disabled Tile")] public Material disabledMaterial;
    #endregion
    #region Interactors
    [Foldout("Filons Prefabs")] public Interactor treePrefab;
    [Foldout("Filons Prefabs")] public Interactor rockPrefab;
    [Foldout("Filons Prefabs")] public Interactor goldPrefab;
    [Foldout("Filons Prefabs")] public Interactor diamondPrefab; 
    [Foldout("Filons Prefabs")] public Interactor adamantiumPrefab;
    #endregion
    #region Components
    [Foldout("Component")] public Transform visualRoot;
    [Foldout("Component")] public MeshRenderer myMeshR;
    [Foldout("Component")] public MeshFilter myMeshF;
    [Foldout("Component")] public MeshCollider myMeshC;
    [Foldout("Component")] public Rigidbody rb;
    [Foldout("Component")] public Collider hubCollider;
    [Foldout("Component")] public Transform minableItems;
    [Foldout("Component")] public Transform altSpawnPositions;
    [Foldout("Component")] public Transform tourbillonT;
    [Foldout("Component")] public ParticleSystem undegradableParticleSystem;
    [Foldout("Component")] public ParticleSystem sandParticleSystem;
    [Foldout("Component")] public MeshRenderer walkRunes;
    [Foldout("Component")] public ParticleSystem pSysCreation;
    [Foldout("Component")] public GameObject pSysCenterTile;
    [Foldout("Component")] public Tile_Degradation tileD;
    [Foldout("Component")] public LevelUI levelUI;
    [Foldout("Component")] public ParticleSystem huizinga;
    #endregion
}
