using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LevelUI : MonoBehaviour
{
    [HideNormalInspector] public Transform mainCamera;
    public GameObject NearUI;
    public GameObject DetailUI;
    public GameObject NoDetailUI;
    public RawImage levelIcon;

    private RessourcesManager rMan;
    Tile tile;

    public void OnActivated()
    {
        tile = GetComponentInParent<Tile>();
        rMan = RessourcesManager.Instance;
        if (Camera.main) mainCamera = Camera.main.transform;
       
        TileSystem.Instance.cam.RenderTextureOnImage(levelIcon, tile.levelName);
    }

    void Update()
    {
        while(mainCamera == null) OnActivated();
        //transform.position = cam.targetGroup.transform.position;
        if (mainCamera) transform.GetChild(0).LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }

    public void PlayerNear()
    {
        NearUI.gameObject.SetActive(true);
    }

    public void PlayerFar()
    {
        NearUI.gameObject.SetActive(false);
    }

    public void NoDetail()
    {
        DetailUI.gameObject.SetActive(false);
        NoDetailUI.gameObject.SetActive(true);
    }

    public void Detail()
    {
        DetailUI.gameObject.SetActive(true);
        NoDetailUI.gameObject.SetActive(false);
    }
}
