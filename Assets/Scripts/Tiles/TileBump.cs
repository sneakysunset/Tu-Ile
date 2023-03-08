using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBump : MonoBehaviour
{
    List<Collider> hitList;
    public float bumpStrength = 5;
    public AnimationCurve bumpDistanceCurve;

    private Tile tile;
    Rigidbody rb;
    private void Start()
    {
        hitList = new List<Collider>();
        tile = GetComponent<Tile>();
        rb = tile.rb;
    }

    public void Bump()
    {
        RaycastHit[] hits;
        hits = rb.SweepTestAll(Vector3.up, 30);

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                hitList.Add(hit.collider);
            }
        }

        foreach (Collider hit in hitList)
        {
            float distance = bumpDistanceCurve.Evaluate(Mathf.Clamp(1 / (hit.transform.position.y - tile.transform.position.y - 45) * 5, 0, 1));
            print(hit.transform.position.y + " " +  tile.transform.position.y);
            if (hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rbx))
            {
                rbx.AddForce(Vector3.up * distance * bumpStrength, ForceMode.Impulse);
            }
            else if (hit.transform.TryGetComponent<PlayerMovement>(out PlayerMovement charC))
            {
                charC._velocity += distance * bumpStrength / 5;
            }
        }

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                hitList.Remove(hit.collider);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rbx) || TryGetComponent<CharacterController>(out CharacterController charC))
        {
            hitList.Add(collision.collider);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rbx) || TryGetComponent<CharacterController>(out CharacterController charC))
        {
            if (hitList.Contains(collision.collider))
            {
                hitList.Remove(collision.collider);
            }
        }
    }
}
