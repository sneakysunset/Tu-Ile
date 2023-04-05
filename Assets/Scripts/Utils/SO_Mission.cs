using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[CreateAssetMenu(fileName = "Scriptable Objects", menuName = "Missions", order = 1)]
public class SO_Mission : ScriptableObject
{
    public string description;
    public int requiredNumber;
    public Tile.TileType requiredType;
    [HideInInspector] public Image missionChecker;
    [HideInInspector] public TextMeshProUGUI missionText;
}
