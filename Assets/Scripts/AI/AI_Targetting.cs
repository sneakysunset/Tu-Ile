using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class AI_Targetting : MonoBehaviour
{
    AI_Behaviour AIB;
    AI_Movement AIM;
    //public float timerDeterioration;
    public float distanceToBait;
    [HideNormalInspector] public Player[] players;
    void Start()
    {
        players = FindObjectsOfType<Player>();
        AIB = GetComponent<AI_Behaviour>(); 
        AIM = GetComponent<AI_Movement>();
    }


    void Update()
    {
        float distance = Mathf.Infinity;
        AIM.Target = null;
        foreach(Player player in players)
        {
            if(player.heldItem && player.heldItem.GetType() == typeof(Item_Bait))
            {
                float tempDist = Vector3.Distance(player.transform.position, transform.position);
                if (tempDist < distanceToBait && tempDist < distance)
                {
                    AIM.Target = player.transform;
                }
            }
        }

        if(AIM.Target != null && AIB.currentBehaviour != AI_Behaviour.Behavious.Target && AIB.currentBehaviour != AI_Behaviour.Behavious.Disable) 
        {
            AIB.currentBehaviour = AI_Behaviour.Behavious.Target;
        }
        else if(AIM.Target == null && AIB.currentBehaviour != AI_Behaviour.Behavious.Disable)
        {
            AIB.currentBehaviour = AI_Behaviour.Behavious.AI;
        }
        

/*        if (AIB.tileUnder && AIB.tileUnder.walkedOnto && !AIB.tileUnder.isGrowing && AIB.tileUnder.degradable)
        {
            AIB.tileUnder.timer -= timerDeterioration * Time.deltaTime;
            AIB.tileUnder.isPenguined = true;
        }*/
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(Selection.objects.Contains(this.gameObject as Object))
        {
            //Gizmos.DrawWireSphere(transform.position, distanceToBait);
        }
    }
#endif
}
