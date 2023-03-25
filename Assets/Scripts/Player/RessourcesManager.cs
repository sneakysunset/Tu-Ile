using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct ressourceMeshsCollec
{
    public string name;
    public Item_Stack.StackType stackType;
    public List<Mesh> meshs;
    public List<Material> materials;
    public List<int> necessaryNum;

}

public class RessourcesManager : MonoBehaviour
{
    public int growthCost;
    public List<ressourceMeshsCollec> RessourceMeshs;

    private void Update()
    {
    }
}
