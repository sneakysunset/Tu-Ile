using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct rewardStruct
{
    public int number;
    public Item rewardItem;
}

public class Item_Crate : Item
{
    public bool itemReward;
    public enum PlayerStateReward {None, Speed, MiningSpeed};
    public PlayerStateReward playerStateReward;
    public List<rewardStruct> itemRewards;
    public int scoreReward;
    public bool isRandom;

    public float yLerpPositionAmount;
    public float lerpDuration;
    public float yLerpRotateAmount;
    public AnimationCurve lerpCurve;

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
        rb.isKinematic = true;
        transform.DOMoveY(transform.position.y + yLerpPositionAmount, lerpDuration).SetEase(lerpCurve);
        transform.DORotate(new Vector3(0, yLerpRotateAmount, 0), lerpDuration, RotateMode.FastBeyond360);
        yield return new WaitForSeconds(lerpDuration);
        transform.DOScale(transform.localScale * 2f, 1);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
        GiveRewards();
    }
    
    private void GiveRewards()
    {
        ScoreManager.Instance.ChangeScore(scoreReward);
        if (itemReward)
        {
            if (!isRandom)
            {
                if (itemRewards[0].rewardItem.GetType() == typeof(Item_Stack))
                {
                    Item_Stack it = Instantiate(itemRewards[0].rewardItem, transform.position, Quaternion.identity) as Item_Stack;
                    it.numberStacked = itemRewards[0].number;
                }
                else
                {

                }
            }

        }
    }

}
