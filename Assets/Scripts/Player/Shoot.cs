using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : MonoBehaviour
{
    bool shootBool;
    public float shootRate = .2f;
    private float timer;
    public ParticleSystem pSys;
    public float bulletWidth;
    public LayerMask hitLayer;
    public int damage = 3;
    private void Start()
    {
        timer = shootRate;
    }

    public void OnShoot(InputAction.CallbackContext cbx)
    {
        if (cbx.started)
        {
            shootBool = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Pistol/Bullet Shot");
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        Debug.DrawRay(transform.position, GetMouseToPlayerDirection() * 10, Color.red, Time.deltaTime) ;
        
        if (shootBool && timer <= 0)
        {
            
            RaycastHit hit;
            var direction = GetMouseToPlayerDirection();
            if (Physics.SphereCast(pSys.transform.position, bulletWidth, direction, out hit, 100,  hitLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.TryGetComponent<UnitBehaviour>(out UnitBehaviour unit))
                {
                    unit.OnHit(damage);
                }
            }
            pSys.transform.parent.forward = direction;
            pSys.Emit(1);
            shootBool = false;
        }
    }

    public  LayerMask mouseLayer;

    Vector3 GetMouseToPlayerDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePos = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,mouseLayer, QueryTriggerInteraction.Collide))
        {
            mousePos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
        Vector3 direction = (mousePos - transform.position).normalized;
        return direction;
    }
}
