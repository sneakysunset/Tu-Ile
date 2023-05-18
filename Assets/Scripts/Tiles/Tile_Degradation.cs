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

    private void Awake()
    {
        spawnPos = transform.GetChild(0).GetChild(0);

        tile = GetComponent<Tile>();

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
        if(transform.position == tile.currentPos) { tile.movingP = false; }
        /*if(!TileSystem.Instance.ready && tile.readyToRoll && !FMODUtils.IsPlaying(tfFI))
        {
            FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
        }

        if(!TileSystem.Instance.ready && transform.position == tile.currentPos)
        {
            FMODUtils.StopFMODEvent(ref tfFI, true);
        }

        if(FMODUtils.IsPlaying(tfFI)) tfFI.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

        if (transform.position == tile.currentPos && !to)
        {
            to = true;
            FMODUtils.StopFMODEvent(ref tfFI, true);
        }*/

        /*if(to == true && !TileSystem.Instance.ready)
        {
            to = false;
            FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
        }*/

        if (transform.position == tile.currentPos) started = true;
        if(tile.isGrowing) Elevating();
        else if (tile.walkable && tile.degradable && tile.walkedOnto && tile.tileType != TileType.Sand) Degrading();
        if (started && !tile.isDegrading && tile.walkable && ((transform.position.y <= -tile.heightByTile && tile.currentPos.y <= -tile.heightByTile)))
        {
            SinkTile();
        }
        if (tile.tileType == TileType.Sand)
        {
            SandDegradation();
        }
    }

    private void SandDegradation()
    {
        if(!tile.sand_WalkedOnto && walkedOntoChecker)
        {
            tile.currentPos.y -= tile.heightByTile;
            tile.movingP = true;
        }

        walkedOntoChecker = tile.sand_WalkedOnto;
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
            tile.movingP = true;
            FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
        }

        //DegradationEnd
        if (transform.position == tile.currentPos && tile.isDegrading)
        {
            tile.isDegrading = false;
            tile.movingP = false;
            tile.timer = Random.Range(tile.minTimer, tile.maxTimer);
            FMODUtils.StopFMODEvent(ref tfFI, true);
        }

        if (tile.currentPos.y == GameConstant.maxTileHeight && CompareTag("DegradingTile"))
        {
            tag = "Tile";
        }
    }
    
    private void Elevating()
    {
        if(!FMODUtils.IsPlaying(tfFI))
        {
            FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
        }
        if(transform.position.y == tile.currentPos.y)
        {
            FMODUtils.StopFMODEvent(ref tfFI, true);
            tile.isGrowing = false;
            tile.timer = Random.Range(tile.minTimer, tile.maxTimer);
            tile.movingP = false;
        }
    }

    void SinkTile()
    {
        tile.walkable = false;
        tile.movingP = false;
        gameObject.layer = LayerMask.NameToLayer("DisabledTile");
        tile.myMeshR.enabled = false;
        //GetComponent<Collider>().enabled = false;
        transform.Find("Additional Visuals").gameObject.SetActive(false);
        tile.minableItems.gameObject.SetActive(false);
        tile.tileS.tileC.Count();
    }

    public void StartMoveSound()
    {
        FMODUtils.SetFMODEvent(ref tfFI, "event:/Tuile/Tile/Terraformingdown", spawnPos);
        StartCoroutine(TileShake(.1f));
    }

    public void EndMoveSound()
    {
        FMODUtils.StopFMODEvent(ref tfFI, true);
    }

    public IEnumerator TileShake(float magnitude)
    {
        while (transform.position.y != tile.currentPos.y)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(tile.currentPos.x + x, transform.position.y, tile.currentPos.z + y);

            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = tile.currentPos;
    }
}
