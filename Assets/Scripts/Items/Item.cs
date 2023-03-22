using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool holdable;
    [HideNormalInspector] public bool isHeld;
    Rigidbody rb;
    [HideInInspector] public GameObject Highlight;
    Transform heldPoint;
    public enum ItemType {Neutral, Wood, Rock};
    public ItemType itemType = ItemType.Neutral;

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
        isHeld = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        //FMODUnity.RuntimeManager.PlayOneShot("event:/MouvementCharacter/Grab");
        player.holdableItems.Add(this);
        transform.parent = null;
    }
}
