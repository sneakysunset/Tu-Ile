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
    public float amountToGoLeft;
    private float targetX;
    private float ogX;
    bool firstTime = true;
    public IEnumerator enumer;
    [SerializeField] private Tile endTutoCenterTile;
    [SerializeField] private Tile startTutoCenterTile;
    [SerializeField] private bool activatTuto;
    private void Start()
    {
        tutoTextTr = text.transform.parent as RectTransform;
        if(activatTuto)
        {
            TileSystem.Instance.centerTile = startTutoCenterTile;
            startTutoCenterTile.tc.myMeshR.materials = startTutoCenterTile.getCorrespondingMat(startTutoCenterTile.tileType);
        }
        else
        {
            tutoTextTr.gameObject.SetActive(false);
            GetComponent<HubEvents>().tileGrowEventList.RemoveAt(0);
            return;
        }
        targetX = tutoTextTr.anchoredPosition.x + amountToGoLeft;
        ogX = tutoTextTr.anchoredPosition.x;
        enumer = GetTree();
        StartCoroutine(enumer);
    }

    public IEnumerator GetTree()
    {
        interactor.isTuto = true;
        if (!firstTime)
        {
            TileSystem.Instance.centerTile.tc.pSysCenterTile.gameObject.SetActive(false);
            TileSystem.Instance.centerTile = centerTile.GetComponentInParent<Tile>();
            TileSystem.Instance.centerTile.tc.myMeshR.materials = TileSystem.Instance.centerTile.getCorrespondingMat(TileSystem.Instance.centerTile.tileType);
            TileSystem.Instance.centerTile.tc.myMeshF.mesh = TileSystem.Instance.centerTile.getCorrespondingMesh(TileSystem.Instance.centerTile.tileType);
            tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
            targetter.DOMove(interactor.target.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
            yield return new WaitForSeconds(timeToGoOut);
        } 
        else
        {
            
            tutoTextTr.anchoredPosition = new Vector2(targetX, tutoTextTr.anchoredPosition.y);
            firstTime = false;
            yield return new WaitUntil(()=>TileSystem.Instance.ready);
            targetter.DOMove(interactor.target.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
            //Light light = tutoTextTr.GetComponentInChildren<Light>();
            //DOVirtual.Float(0f, 50, 1, v => light.intensity = v);
        }
        text.text = "Couper l'arbre en vous placant devant lui";
        tutoTextTr.DOAnchorPosX(ogX, timeToGoIn);
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
        tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Ramasser le bois avec le bouton 'X'. Puis amener le bois � l'�tabli et d�poser le avec le bouton 'X'";
        tutoTextTr.DOAnchorPosX(ogX, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        enumer = null;
    }

    public IEnumerator GetChantier()
    {
        chantier.isTuto = true;
        chantier.interactable = true;

        targetter.DOMove(chantier.targetter.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Amener le bois au chantier et d�posez le avec le bouton 'X'";
        tutoTextTr.DOAnchorPosX(ogX, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        chantier.interactable = true;
        enumer = null;
    }

    public IEnumerator GetCenterTile()
    {

        targetter.DOMove(centerTile.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);

        tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Amener le coffre � la tile avec une cercle magique et d�posez la avec le bouton 'X'";
        tutoTextTr.DOAnchorPosX(ogX, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        enumer = null;
    }   

    public IEnumerator GetSunkTile()
    {
        targetter.DOMove(sunkTile.position, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
        yield return new WaitForSeconds(timeToGoOut);
        text.text = "Utiliser la tuile de bois avec le bouton 'RT' pour cr�er une plateforme dans l'eau";
        tutoTextTr.DOAnchorPosX(ogX, timeToGoIn);
        yield return new WaitForSeconds(timeToGoIn);
        //targetter.position = sunkTile.position;
        enumer = null;
    }

    public void ExitTuto()
    {
        TileSystem.Instance.centerTile.tc.pSysCenterTile.gameObject.SetActive(false);
        TileSystem.Instance.centerTile = endTutoCenterTile;
        targetter.DOMove(Vector3.zero, timeToGoIn + timeToGoOut).SetEase(TileSystem.Instance.easeInOut);
        tutoTextTr.DOAnchorPosX(targetX, timeToGoOut);
    }
}
