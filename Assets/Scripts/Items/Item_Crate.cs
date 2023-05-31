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
    public SO_CrateReward reward;
    public enum PlayerStateReward { None, Speed, MiningSpeed };


    [Foldout("Lerp")] public float yLerpPositionAmount;
    [Foldout("Lerp")] public float ScaleAmount = 2;
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
            StartCoroutine(OnCenterReached(tileUnder.minableItems));
        }
    }

    public IEnumerator OnCenterReached(Transform pos)
    {
        holdable = false;
        transform.position = pos.position;
        rb.isKinematic = true;
        transform.DOMoveY(transform.position.y + yLerpPositionAmount + transform.localScale.y, lerpDuration).SetEase(lerpCurve);
        transform.DORotate(new Vector3(0, yLerpRotateAmount, 0), lerpDuration, RotateMode.FastBeyond360);
        yield return new WaitForSeconds(lerpDuration);
        transform.DOScale(transform.localScale * ScaleAmount, 1);
        yield return new WaitForSeconds(1);
        GiveRewards();
        Destroy(gameObject);
    }
    
    private void GiveRewards()
    {
        int i = 0;
        TileSystem.Instance.scoreManager.ChangeScore(reward.scoreReward);
        if (reward.itemReward)
        {
            if (reward.isRandom)
            {
                i = UnityEngine.Random.Range(0, reward.itemRewards.Count - 1);
            }

            if (Utils.IsSameOrSubclass(typeof(Item_Stack), reward.itemRewards[i].rewardItem.GetType()))
            {
                Item_Stack it = Instantiate(reward.itemRewards[i].rewardItem, transform.position, Quaternion.identity) as Item_Stack;
                it.numberStacked = reward.itemRewards[i].number;
            }
            else
            {
                Instantiate(reward.itemRewards[i].rewardItem, transform.position, Quaternion.identity);
            }

        }
        else
        {
            if (reward.isRandom) 
            {
                i = UnityEngine.Random.Range(0, reward.playerStateReward.Length - 1);
            }


            foreach(Player p in FindObjectsOfType<Player>())
            {
                switch(reward.playerStateReward[i].pReward)
                {
                    case PlayerStateReward.None: 
                    
                        break;
                    case PlayerStateReward.Speed:
                        p.pM.speed += reward.playerStateReward[i].rewardAdd;
                        p.pM.speedOnRocks += reward.playerStateReward[i].rewardAdd;
                        break;
                    case PlayerStateReward.MiningSpeed:
                        p.pMin.hitRate += reward.playerStateReward[i].rewardAdd;
                        break;
                }
            }
        }
    }

}
