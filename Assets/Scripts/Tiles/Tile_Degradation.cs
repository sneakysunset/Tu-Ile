using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile_Degradation : MonoBehaviour
{
    //bool started = false;
    #region Variables
    Tile t;
    TileComponentReferencer tc;
    FMOD.Studio.EventInstance tfFI;
    [HideNormalInspector] public float tileDegraderMult = 1;
    [HideNormalInspector] private bool isDegrading;
    private IEnumerator shakeCor;


    #region SerializedFields
    [SerializeField, NaughtyAttributes.ProgressBar(1)] private float degradationTimer;
    private float timerValue;
    public float degradationTimerMin, degradationTimerMax;
    [SerializeField] private AnimationCurve degradationTimerAnimCurve;
    public float heightByTile;
    [SerializeField, Range(0, 1)] private float shakeActivationTimePercent = .3f;
   // [SerializeField] private float shakeFrequency = .03f;
    [SerializeField] private float shakeMagnitude = .1f;
    [SerializeField] private AnimationCurve shakeCurve;
    [SerializeField, Range(0, 10)] private float degradationLerpSpeed;
    [SerializeField, Range(0, 10)] private float growthLerpSpeed;
    #endregion
    #endregion

    private void Awake()
    {
        degradationTimer = 1;
        t = GetComponent<Tile>();
        tc = GetComponent<TileComponentReferencer>();
        tileDegraderMult = 1;
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
        if (transform.position.y == t.currentPos.y) t.IsMoving = false;
        else t.IsMoving = true;


        if (FMODUtils.IsPlaying(tfFI)) tfFI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(tc.minableItems));

        if(isDegrading && t.tileType != TileType.Sand) degradationTimer -= Time.deltaTime * t.typeDegradingSpeed / timerValue;
        if(isDegrading && degradationTimer < 0) 
        {
            t.currentPos.y -= heightByTile;
            isDegrading = false;
        }
        if(isDegrading && degradationTimer < shakeActivationTimePercent * t.typeDegradingSpeed && shakeCor == null)
        {
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }
    }

    private void FixedUpdate()
    {
        if (TileSystem.Instance.ready /*&& tile.readyToRoll*/ && t.IsMoving)
        {
            Vector3 localPos = t.transform.localPosition;
            float distance = Mathf.Abs(t.transform.position.y - t.currentPos.y);
            distance = Mathf.Clamp(distance, .3f, 3f);

            if (t.isGrowing)
            {
                tc.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, t.currentPos.y, localPos.z), (1 / growthLerpSpeed) * Time.deltaTime * distance * tileDegraderMult));
            }
            else
            {
                tc.rb.MovePosition(Vector3.MoveTowards(localPos, new Vector3(localPos.x, t.currentPos.y, localPos.z), degradationLerpSpeed * Time.deltaTime * t.degSpeed * TileSystem.Instance.lerpingSpeed * distance * tileDegraderMult));
            }
        }
    }

    public void SandDegradation()
    {
        if(shakeCor == null && !TileSystem.Instance.isHub)
        {
            t.currentPos.y -= t.td.heightByTile;
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }
    }

    public void StartDegradation()
    {
        timerValue = Random.Range(degradationTimerMin, degradationTimerMax);
        degradationTimer = 1;
        isDegrading = true;
    }

    public void EndDegradation()
    {
        isDegrading = false;
/*        if (degradationCor != null)
        {
            StopCoroutine(degradationCor);
            degradationCor = null;
        }*/
    }

/*    private IEnumerator Degrading()
    {
        print("StartDegradation" + gameObject.name);
        //Effect while degrading
        yield return TileSystem.Instance.shakeLongWaiter;
        if (!TileSystem.Instance.isHub)
        {
            shakeCor = TileShake();
            StartCoroutine(shakeCor);
        }
        else
        {
            degradationCor = null;
            yield break;
        }

        float timeBeforeDegradation = Random.Range(degradationTimerMin, degradationTimerMax) / t.typeDegradingSpeed / TileSystem.Instance.degradationTimerModifier - TileSystem.Instance.tileP.shakeActivationTime;
        yield return new WaitForSeconds(timeBeforeDegradation);

        t.isDegrading = true;
        gameObject.tag = "DegradingTile";
        print("Degradation" + gameObject.name);
        if (!TileSystem.Instance.isHub) t.currentPos.y -= t.td.heightByTile;
        degradationCor = null;
    }*/
    
    void SinkTile()
    {
        t.walkable = false;
        t.currentPos.y = -16f;
        gameObject.layer = LayerMask.NameToLayer("DisabledTile");
        //tile.myMeshR.enabled = false;
        //transform.Find("Additional Visuals").gameObject.SetActive(false);
        //tile.minableItems.gameObject.SetActive(false);
        if (t.tileType == TileType.Sand) transform.Find("SandParticleSystem").GetComponent<ParticleSystem>().Stop();
        if (t.tileType == TileType.BouncyTile) tc.rb.isKinematic = true;
        if(!TileSystem.Instance.isHub) TileSystem.Instance.tileCounter.Count();
    }

    public void StartTileMovement()
    {
        FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", tc.minableItems);
    }

    public void EndTileMovement()
    {
        tileDegraderMult = 1;
        FMODUtils.StopFMODEvent(ref tfFI, true);
        if(shakeCor != null ) StopCoroutine(shakeCor);
        shakeCor = null;
        if (t.isGrowing) t.isGrowing = false;
        else if (t.isDegrading) t.isDegrading = false;
        t.timer = UnityEngine.Random.Range(degradationTimerMin, degradationTimerMax);
        if(t.currentPos.y == GameConstant.maxTileHeight && CompareTag("DegradingTile")) tag = "Tile";
        if (t.walkable && t.currentPos.y < 0) SinkTile();
        t.sandFlag = false;
        if(t.degradable && t.walkable && t.tileType != TileType.Sand && t.walkedOnto && !TileSystem.Instance.isHub) StartDegradation();
    }

    public IEnumerator TileShake()
    {
        Vector3 pos = transform.localPosition;
        while(isDegrading)
        {
            transform.localPosition = ShakeEffect(1 - degradationTimer, pos);
            yield return TileSystem.Instance.shakeWaiter;
        }
       // yield return new WaitUntil(() => transform.position != t.currentPos);
        while (transform.position != t.currentPos)
        {
            //transform.localPosition = ShakeEffect(1, pos);
            yield return TileSystem.Instance.shakeWaiter;
        }
        shakeCor = null;
    }

    private Vector3 ShakeEffect(float curveTime, Vector3 ogPos)
    {
        float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude * shakeCurve.Evaluate(curveTime);
        float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude * shakeCurve.Evaluate(curveTime);
        return new Vector3(ogPos.x + x, transform.localPosition.y, ogPos.z + y);
    }
}
