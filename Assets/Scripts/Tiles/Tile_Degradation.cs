using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile_Degradation : MonoBehaviour
{
    Tile tile;
    bool started = false;
    FMOD.Studio.EventInstance tfFI;
    Transform spawnPos;
    bool to;
    [HideInInspector] public IEnumerator shakeCor;
    [HideInInspector] public IEnumerator degradationCor;
    public float sFre;

    private void Awake()
    {
        tile = GetComponent<Tile>();
        spawnPos = tile.minableItems.GetChild(0);
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


        if (FMODUtils.IsPlaying(tfFI)) tfFI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(spawnPos));
        //if (tile.walkable && tile.degradable && tile.walkedOnto && tile.tileType != TileType.Sand) Degrading();

/*        if(tile.timer < tile.shakeActivationTime && shakeCor == null)
        {
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }*/
    }


    private void FixedUpdate()
    {
        if (TileSystem.Instance.ready /*&& tile.readyToRoll*/ && tile.IsMoving)
        {
            Vector3 localPos = tile.transform.localPosition;
            float distance = Mathf.Abs(tile.transform.position.y - tile.currentPos.y);
            distance = Mathf.Clamp(distance, .3f, 5f);

            if (tile.isGrowing)
            {
                tile.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), (1 / TileSystem.Instance.tileMov.tileGrowthLerpSpeed) * Time.deltaTime * distance));
            }
            else
            {
                tile.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, tile.currentPos.y, localPos.z), TileSystem.Instance.tileMov.degradationLerpSpeed * Time.deltaTime * tile.degSpeed * TileSystem.Instance.lerpingSpeed * distance));
            }
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

    public void StartDegradation()
    {
        if(degradationCor != null)
        {
            StopCoroutine(degradationCor);
        }
        degradationCor = Degrading();
        StartCoroutine(degradationCor);
    }

    public void EndDegradation()
    {
        if (degradationCor != null)
        {
            StopCoroutine(degradationCor);
            degradationCor = null;
        }
    }


    private IEnumerator Degrading()
    {
        //Effect while degrading
        
        yield return TileSystem.Instance.shakeLongWaiter;

        shakeCor = TileShake();
        StartCoroutine(shakeCor);

        float timeBeforeDegradation = Random.Range(tile.degradationTimerMin, tile.degradationTimerMax) / tile.typeDegradingSpeed / TileSystem.Instance.degradationTimerModifier - TileSystem.Instance.tileP.shakeActivationTime;
        yield return new WaitForSeconds(timeBeforeDegradation);

/*        if (tile.timer > 0)
        {
            tile.timer -= Time.deltaTime * TileSystem.Instance.degradationTimerModifier * tile.typeDegradingSpeed;
        }*/
        //DegradationStart
/*        else if (tile.timer <= 0 && !tile.isDegrading)
        {*/
            tile.isDegrading = true;
            gameObject.tag = "DegradingTile";
            tile.currentPos.y -= tile.heightByTile;
        //}
        degradationCor = null;
    }
    
    void SinkTile()
    {
        tile.walkable = false;
        tile.currentPos.y = -16f;
        gameObject.layer = LayerMask.NameToLayer("DisabledTile");
        //tile.myMeshR.enabled = false;
        //transform.Find("Additional Visuals").gameObject.SetActive(false);
        //tile.minableItems.gameObject.SetActive(false);
        if (tile.tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Stop();
        if (tile.tileType == TileType.BouncyTile) tile.rb.isKinematic = true;
        if(!TileSystem.Instance.isHub) TileSystem.Instance.tileCounter.Count();
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
        tile.timer = UnityEngine.Random.Range(tile.degradationTimerMin, tile.degradationTimerMax);
        if(tile.currentPos.y == GameConstant.maxTileHeight && CompareTag("DegradingTile")) tag = "Tile";
        started = true;
        if (started && tile.walkable && tile.currentPos.y < 0) SinkTile();
        tile.sandFlag = false;
        if(tile.degradable && tile.walkable && tile.tileType != TileType.Sand && tile.walkedOnto && !TileSystem.Instance.isHub) StartDegradation();
    }

    public IEnumerator TileShake()
    {
        float f = 0;
        while(f < 1)
        {
            f += Time.deltaTime * (1 / TileSystem.Instance.tileP.shakeActivationTime);
            tile.visualRoot.localPosition = ShakeEffect(f);
            yield return TileSystem.Instance.shakeWaiter;
        }
        yield return new WaitUntil(() => transform.position != tile.currentPos);
        while (transform.position != tile.currentPos)
        {
            Debug.Log(f);
            tile.visualRoot.localPosition = ShakeEffect(f);
            yield return TileSystem.Instance.shakeWaiter;
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
