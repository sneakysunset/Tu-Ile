using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperEffect : MonoBehaviour
{
    bool isCloseEnought;
    Transform _target;
    public float rushSpeed;
    AI_Behaviour AIB;


    public void TargetPlayer(Transform target, AI_Behaviour AI)
    {
        _target = target;
        isCloseEnought = true;
        AIB = AI;
        AIB.ClearPath();
    }

    private void Update()
    {
        if(isCloseEnought)
        {
            transform.position = Vector3.MoveTowards(transform.position, _target.position, rushSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AIB.tileUnder.walkable = false;
            Destroy(this.gameObject);
        }
    }
}
