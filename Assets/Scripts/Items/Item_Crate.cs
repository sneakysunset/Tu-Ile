using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct rewardStruct
{
    public int number;
    public Item rewardItem;
}

[System.Serializable]
public struct rewardPStateStruct
{
    public float rewardAdd;
    public Item_Crate.PlayerStateReward pReward;
}

public class Item_Crate : Item
{
    public enum PlayerStateReward {None, Speed, MiningSpeed};
    public bool itemReward;
    public bool isRandom;
    public int scoreReward;
    public ParticleSystem OnActivation;
    [HideIf("itemReward")]public rewardPStateStruct[] playerStateReward;
    [ShowIf("itemReward")] public List<rewardStruct> itemRewards;


    [Foldout("Lerp")] public float yLerpPositionAmount;
    [Foldout("Lerp")] public float lerpDuration;
    [Foldout("Lerp")] public float yLerpRotateAmount;
    [Foldout("Lerp")] public AnimationCurve lerpCurve;

    public override void GrabStarted(Transform holdPoint, Player player)
    {
        base.GrabStarted(holdPoint, player);
    }

    public override void GrabRelease(bool etablied)
    {
        base.GrabRelease(etablied);
        Tile tileUnder = GridUtils.WorldPosToTile(this.gameObject.transform.position);
        if(tileUnder == TileSystem.Instance.centerTile)
        {
            StartCoroutine(OnCenterReached(tileUnder.transform.GetChild(0)));
        }
    }

    public IEnumerator OnCenterReached(Transform pos)
    {
        transform.position = pos.position;
        var part = Instantiate(OnActivation, transform.position, Quaternion.identity);
        rb.isKinematic = true;
        transform.DOMoveY(transform.position.y + yLerpPositionAmount, lerpDuration).SetEase(lerpCurve);
        transform.DORotate(new Vector3(0, yLerpRotateAmount, 0), lerpDuration, RotateMode.FastBeyond360);
        yield return new WaitForSeconds(lerpDuration);
        transform.DOScale(transform.localScale * 2f, 1);
        yield return new WaitForSeconds(1);
        GiveRewards();
        Destroy(part);
        Destroy(gameObject);
    }
    
    private void GiveRewards()
    {
        int i = 0;
        ScoreManager.Instance.ChangeScore(scoreReward);
        if (itemReward)
        {
            if (isRandom)
            {
                i = UnityEngine.Random.Range(0, itemRewards.Count - 1);
            }

            if (Utils.IsSameOrSubclass(typeof(Item_Stack), itemRewards[i].rewardItem.GetType()))
            {
                Item_Stack it = Instantiate(itemRewards[i].rewardItem, transform.position, Quaternion.identity) as Item_Stack;
                it.numberStacked = itemRewards[i].number;
            }
            else
            {
                Instantiate(itemRewards[i].rewardItem, transform.position, Quaternion.identity);
            }

        }
        else
        {
            if (isRandom) 
            {
                i = UnityEngine.Random.Range(0, playerStateReward.Length - 1);
            }


            foreach(Player p in FindObjectsOfType<Player>())
            {
                switch(playerStateReward[i].pReward)
                {
                    case PlayerStateReward.None: 
                    
                        break;
                    case PlayerStateReward.Speed:
                        p.pM.speed += playerStateReward[i].rewardAdd;
                        p.pM.speedOnRocks += playerStateReward[i].rewardAdd;
                        break;
                    case PlayerStateReward.MiningSpeed:
                        p.pMin.hitRate += playerStateReward[i].rewardAdd;
                        break;
                }
            }
        }
    }

}
