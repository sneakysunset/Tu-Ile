using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool holdable;
    [HideNormalInspector] public bool isHeld;
    protected Rigidbody rb;
    [HideInInspector] public GameObject Highlight;
    protected Transform heldPoint;
    protected Player _player;
    public int numberStacked = 0;
    public virtual void Awake()
    {
        Highlight = transform.Find("Highlight").gameObject;

        if (TryGetComponent<Rigidbody>(out rb)) { }
    }

    public virtual void Update()
    {

    }

    public virtual void GrabStarted(Transform holdPoint, Player player)
    {
        _player = player;
        isHeld = true;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        //FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCharacter/Catch");
        rb.isKinematic = true;
        if (player.holdableItems.Contains(this))
            player.holdableItems.Remove(this);
        heldPoint = holdPoint;
        Highlight.SetActive(false);
        transform.position = heldPoint.position;
        transform.parent = heldPoint;
    }

    public virtual void GrabRelease(Player player)
    {
        _player = null;
        isHeld = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        //FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCharacter/Grab");
        player.holdableItems.Add(this);
        transform.parent = null;
    }


    public virtual void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Water"))
        {
            if (_player)
            {
                _player.heldItem = null;
                if (_player.holdableItems.Contains(this))
                {
                    _player.holdableItems.Remove(this);
                }
            }
            Destroy(gameObject);
        }
    }
}
