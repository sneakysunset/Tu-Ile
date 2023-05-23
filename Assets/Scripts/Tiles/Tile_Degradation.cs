using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile_Degradation : MonoBehaviour
{
    Tile tile;
    [HideNormalInspector] private bool walkedOntoChecker;
    bool started = false;
    FMOD.Studio.EventInstance tfFI;
    Transform spawnPos;
    bool to;
    IEnumerator shakeCor;
    public float sFre;
    private void Awake()
    {
        spawnPos = transform.GetChild(0).GetChild(0);
        tile = GetComponent<Tile>();
        if (tile.visualRoot == null) tile.visualRoot = transform.Find("TileVisuals"); 
    }

    private void OnDestroy()
    {
        if(FMODUtils.IsPlaying(tfFI))
        {
            FMODUtils.StopFMODEvent(ref tfFI, true);
        }
    }

    private void Update()
    {
        if (transform.position.y == tile.currentPos.y) tile.IsMoving = false;
        else tile.IsMoving = true;

        if (tile.walkable && tile.degradable && tile.walkedOnto && tile.tileType != TileType.Sand) Degrading();

        if(tile.timer < tile.shakeActivationTime && shakeCor == null)
        {
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }
    }

    public void SandDegradation()
    {
        if(shakeCor == null)
        {
            tile.currentPos.y -= tile.heightByTile;
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }
    }

    private void Degrading()
    {
        //Effect while degrading
        if (tile.timer > 0)
        {
            tile.timer -= Time.deltaTime * TileSystem.Instance.degradationTimerModifier * tile.typeDegradingSpeed;
        }
        //DegradationStart
        else if (tile.timer <= 0 && !tile.isDegrading)
        {
            tile.isDegrading = true;
            gameObject.tag = "DegradingTile";
            tile.currentPos.y -= tile.heightByTile;
        }
    }
    

    void SinkTile()
    {
        tile.walkable = false;
        tile.currentPos.y = -5f;
        gameObject.layer = LayerMask.NameToLayer("DisabledTile");
        //tile.myMeshR.enabled = false;
        //transform.Find("Additional Visuals").gameObject.SetActive(false);
        //tile.minableItems.gameObject.SetActive(false);
        TileSystem.Instance.tileC.Count();
    }

    public void StartTileMovement()
    {
        FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
    }

    public void EndTileMovement()
    {
        FMODUtils.StopFMODEvent(ref tfFI, true);
        if(shakeCor != null ) StopCoroutine(shakeCor);
        shakeCor = null;
        if (tile.isGrowing) tile.isGrowing = false;
        else if (tile.isDegrading) tile.isDegrading = false;
        tile.timer = tile.degradationTimer;
        if(tile.currentPos.y == GameConstant.maxTileHeight && CompareTag("DegradingTile")) tag = "Tile";
        started = true;
        if (started && tile.walkable && tile.currentPos.y <= -tile.heightByTile + .3f) SinkTile();
        tile.sandFlag = false;
    }

    public IEnumerator TileShake()
    {
        float f = 0;
        WaitForSeconds waiter = new WaitForSeconds(sFre);
        while(f < 1)
        {
            f += Time.deltaTime * (1 / tile.shakeActivationTime);
            tile.visualRoot.localPosition = ShakeEffect(f);
            yield return waiter;
        }
        yield return new WaitUntil(() => transform.position != tile.currentPos);
        while (transform.position != tile.currentPos)
        {
            tile.visualRoot.localPosition = ShakeEffect(f);
            yield return waiter;
        }
        shakeCor = null;
    }

    private Vector3 ShakeEffect(float curveTime)
    {
        float x = UnityEngine.Random.Range(-1f, 1f) * tile.shakeMagnitude * tile.shakeCurve.Evaluate(curveTime);
        float y = UnityEngine.Random.Range(-1f, 1f) * tile.shakeMagnitude * tile.shakeCurve.Evaluate(curveTime);
        return new Vector3(0 + x, tile.visualRoot.localPosition.y, 0 + y);
    }
}
