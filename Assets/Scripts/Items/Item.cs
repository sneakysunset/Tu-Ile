using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum StackType { Other, Wood, Rock, Gold, Diamond, Adamantium, BouncyTile};
    public enum ItemType { Bird, Chantier, Boussole};
    public bool holdable;
    [HideNormalInspector] public bool isHeld;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public GameObject Highlight;
    protected Transform heldPoint;
    [HideInInspector] public Player _player;
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
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Collect");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Take");
        rb.isKinematic = true;
        if (player.holdableItems.Contains(this))
            player.holdableItems.Remove(this);
        heldPoint = holdPoint;
        Highlight.SetActive(false);
        transform.position = heldPoint.position;
        transform.rotation = heldPoint.rotation;
        transform.Rotate(0, 90, 0);
        transform.parent = heldPoint;
    }

    public virtual void GrabRelease(bool etablied)
    {
        isHeld = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Release");
        _player.holdableItems.Add(this);
        if (!etablied)
        {
            transform.parent = null;
            transform.rotation = Quaternion.identity;
            physic = true;
            Highlight.SetActive(true);            
        }
    }

    public virtual void ThrowAction(Player player, float throwStrength, Vector3 direction)
    {
        isHeld = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Throw");
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
            StartCoroutine(KillItem(other.collider));
        }
    }

    public  virtual IEnumerator KillItem(Collider other)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Drowning", transform.position);
        Physics.IgnoreCollision(other, col, true);
        System.Type type = this.GetType();
        StartCoroutine(MissionManager.Instance.CheckElimMission(type));
        if (_player)
        {
            _player.heldItem = null;
            if (_player.holdableItems.Contains(this))
            {
                _player.holdableItems.Remove(this);
            }
        }
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
