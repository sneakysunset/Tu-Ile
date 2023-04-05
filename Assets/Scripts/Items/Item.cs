using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool holdable;
    [HideNormalInspector] public bool isHeld;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public GameObject Highlight;
    protected Transform heldPoint;
    protected Player _player;
    [HideInInspector] public bool physic = true;
    [HideInInspector] public Collider col;
    public virtual void Awake()
    {
        Highlight = transform.Find("Highlight").gameObject;
        col = GetComponent<Collider>();
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
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Cha_Collect_Item");
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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Cha_Release_Item");
        player.holdableItems.Add(this);
        transform.parent = null;
        transform.rotation = Quaternion.identity;
        physic = true;
    }

    public virtual void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        isHeld = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Cha_Release_Item");
        player.holdableItems.Add(this);
        transform.parent = null;
        transform.rotation = Quaternion.identity;
        rb.AddForce(throwStrength * direction, ForceMode.Impulse);
        physic = true;
    }

    public virtual void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Water"))
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Water_fall");
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
