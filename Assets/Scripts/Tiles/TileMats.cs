using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMats : MonoBehaviour
{
    public Material selectedTileMaterial, unselectedTileMaterial, disabledTileMaterial, FadedTileMaterial;
    public GameObject treePrefab, rockPrefab;
    private TileSystem tileSystem;

    private void OnValidate()
    {
        tileSystem = GetComponent<TileSystem>();
        tileSystem.UpdateParameters = true;
    }
}
