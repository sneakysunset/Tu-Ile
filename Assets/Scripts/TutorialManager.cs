using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public Transform targetter;
    
    private Item_Etabli[] item_Etablis;
    private Item_Etabli etabli;
    private Item_Etabli chantier;
    public Interactor interactor;
    public Transform sunkTile;
    public Transform centerTile;
    [HideInInspector] public bool tileSpawned;
    public TextMeshProUGUI text;
    private RectTransform tutoTextTr;
    public float timeToGoOut, timeToGoIn;
    public float amountToGoUp;
    private float targetY;
    private float ogY;
    bool firstTime = true;
    public IEnumerator enumer;
    private void Start()
    {
        tutoTextTr = text.transform.parent as RectTransform;
        targetY = tutoTextTr.anchoredPosition.y + amountToGoUp;
        ogY = tutoTextTr.anchoredPosition.y;
        enumer = GetTree();
        StartCoroutine(enumer);
    }

    public IEnumerator GetTree()
    {
        interactor.isTuto = true;
        if (!firstTime)
        {
            tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
            targetter.DOMove(interactor.target.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
            yield return new WaitForSeconds(timeToGoOut);
        } 
        else
        {
            
            tutoTextTr.anchoredPosition = new Vector2(tutoTextTr.anchoredPosition.x, targetY);
            firstTime = false;
            yield return new WaitUntil(()=>TileSystem.Instance.ready);
            targetter.DOMove(interactor.target.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
            //Light light = tutoTextTr.GetComponentInChildren<Light>();
            //DOVirtual.Float(0f, 50, 1, v => light.intensity = v);
        }
        text.text = "Couper l'arbre";
        tutoTextTr.DOAnchorPosY(ogY, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        //targetter.position = interactor.target.position;
        enumer = null;
    }

    public IEnumerator GetEtabli()
    {
/*        if (item_Etablis == null)
        {*/
            item_Etablis = FindObjectsOfType<Item_Etabli>();
            etabli = item_Etablis[0];
            chantier = item_Etablis[1];
            if (item_Etablis[0].isChantier)
            {
                etabli = item_Etablis[1];
                chantier = item_Etablis[0];
            }
       // }
        etabli.isTuto = true;
        targetter.DOMove(etabli.targetter.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Amenez le bois à l'établi";
        tutoTextTr.DOAnchorPosY(ogY, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        enumer = null;
    }

    public IEnumerator GetChantier()
    {
        chantier.isTuto = true;
        chantier.interactable = true;
        TileSystem.Instance.centerTile = centerTile.GetComponentInParent<Tile>();
        TileSystem.Instance.centerTile.myMeshR.materials = TileSystem.Instance.centerTile.getCorrespondingMat(TileSystem.Instance.centerTile.tileType);
        TileSystem.Instance.centerTile.myMeshF.mesh = TileSystem.Instance.centerTile.getCorrespondingMesh(TileSystem.Instance.centerTile.tileType);
        targetter.DOMove(chantier.targetter.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Amenez le bois au chantier";
        tutoTextTr.DOAnchorPosY(ogY, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        chantier.interactable = true;
        enumer = null;
    }

    public IEnumerator GetCenterTile()
    {
        targetter.DOMove(centerTile.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);

        tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Amenez le bois à la tile avec une cercle magique";
        tutoTextTr.DOAnchorPosY(ogY, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        enumer = null;
    }

    public IEnumerator GetSunkTile()
    {
        targetter.DOMove(sunkTile.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Utilisez la tuile de bois pour créer une plateforme";
        tutoTextTr.DOAnchorPosY(ogY, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        //targetter.position = sunkTile.position;
        enumer = null;
    }

    public void ExitTuto()
    {
        targetter.DOMove(Vector3.zero, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosY(targetY, timeToGoOut);
    }
}
