using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _characterController;
    private Player player;
    #region Variables: Movement
    [HideInInspector] public Vector2 _input;
    [HideInInspector] public bool jumpInput;
    [HideInInspector] public Vector3 _direction;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool dashFlag;
    [HideInInspector] public bool moveFlag;

    [SerializeField] public float speed;
    [SerializeField] public float speedOnRocks;
    [SerializeField] public float jumpStrength = 10;
    [SerializeField] public float jumpStrengthOnBounce = 20;
    public bool autoJump = true;
    [HideInInspector] private float dashStrength;
     public float pushStrength;
    [HideInInspector] private float dashDuration;
    [HideInInspector] public float dashCooldown = 1;
    [Header("1 = 0.1 sec, .1 = 1 sec")]
    //[SerializeField, Range(0.01f, 1)] private float acceleration = .7f;
    //[SerializeField, Range(0.01f, 1)] private float deceleration = .6f;
    [Space(10)]
    private Vector3 dir;
    private float mvtStr;
    private float speedValue;
    private float dashTimer;
    private float jp;
    private float ms;

    Rigidbody rb;
    [HideNormalInspector] public PlayerMovement otherPM;
    public float stunDuration;
    [HideNormalInspector] public bool isStunned;
    [HideInInspector] public RigidbodyConstraints constraints;
    [HideInInspector] public IEnumerator stunCor;
    #endregion
    #region Variables: Rotation

    [SerializeField] private float smoothTime = 0.05f;
    private float _currentVelocity;

    #endregion
    #region Variables: Gravity
    private bool groundedCallback;
    private float _gravity = -9.81f;
    [SerializeField] public float gravityMultiplier = 3.0f;
    [HideInInspector] public float _velocity;
    [HideNormalInspector] public bool canMove = true;
    private bool groundHit;
    #endregion

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        constraints = rb.constraints;
    }

    private void OnGroundedCallBack()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Stomp");
    }

    private void Update()
    {
        if(!groundHit && _characterController.isGrounded && player.pState == Player.PlayerState.Jump)
        {
            player.pState = Player.PlayerState.Idle;
        }
        groundHit = _characterController.isGrounded;

        if (_characterController.isGrounded && !groundedCallback)
        {
            OnGroundedCallBack();
        }

        groundedCallback = _characterController.isGrounded;
        ApplyGravity();

        if (canMove) ApplyJump();
        ApplyRotation();
        SpeedModifier();
        jumpValueLerp();
        //if(!canMove) _direction = new Vector3(0, _direction.y, 0);
        if (isDashing)
        {
            StartCoroutine(Dash());
            isDashing = false;
        }
        if (dashFlag)
        {
            ApplyDash();
        }
        else if(player._characterController.enabled)
        {
            ApplyMovement();
        }
        if (!canMove && isStunned ) player._characterController.enabled = false;
        dashTimer -= Time.deltaTime;
    }


    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }

        _direction.y = _velocity;
    }

    private void jumpValueLerp()
    {
        if (player.tileUnder && player.tileUnder.tileType == TileType.BouncyTile)
        {
            jp = jumpStrengthOnBounce;
        }
        else
        {
            jp = jumpStrength;
        }
    }

    private void ApplyJump()
    {
        if (_characterController.isGrounded && jumpInput)
        {
            _velocity = jp;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Voix/Jump", transform.localPosition);

            player.pState = Player.PlayerState.Jump;
        }

        jumpInput = false;
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        if (canMove)
        {
            var targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
            player.dummyTarget.rotation = Quaternion.Euler(0, -angle, 0);
        }
    }

    private void SpeedModifier()
    {
        /*        float ax = isDashing ? acceleration : -deceleration;
                speedValue = Mathf.Lerp(speed, sprintingSpeed, ax * Time.deltaTime * 10);*/
        
        if(_characterController.isGrounded && player.tileUnder != null && player.tileUnder.tileType == TileType.Rock)
        {
            speedValue = Mathf.Lerp(speedValue, speedOnRocks, Time.deltaTime * 3);
        }
        else
        {
            speedValue = Mathf.Lerp(speedValue, speed, Time.deltaTime * 3);
        }
    }

    private void ApplyMovement()
    {
        if(!player.isShipped) _characterController.Move(_direction * speedValue * Time.deltaTime);
    }

    private void ApplyDash()
    {
        _characterController.Move(dir * mvtStr * Time.deltaTime) ;
    }
        
        
    private void Move(InputAction.CallbackContext context)
    {


        _input = context.ReadValue<Vector2>();
        if (player.isShipped)
        {
            player.ship._input = _input;
        }
        float cameraAngle = -Camera.main.transform.rotation.eulerAngles.y;
        _input = Rotate(_input, cameraAngle);
        if (!canMove) _input = Vector2.zero;
        _direction = new Vector3(_input.x, 0.0f, _input.y);
        if(player.anim) player.anim.SetFloat("walkingSpeed", _input.magnitude);

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //jumpInput = true;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && dashTimer <= 0 && _characterController.isGrounded)
        {
            jumpInput = true;
            //isDashing = true;
            //dashTimer = dashCooldown;   
        }
        else if (context.canceled || context.performed)
        {
            isDashing = false;
        }
    }
        
    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }



    IEnumerator Dash()
    {
        dashFlag = true;
        dir = transform.forward;
        mvtStr = dashStrength;
        yield return new WaitForSeconds(dashDuration);
        dashFlag = false;
    }
    
    public IEnumerator Dashed(Vector3 vec, float pushStr)
    {
        dashFlag = true;
        mvtStr = pushStr;
        dir = vec;
        yield return new WaitForSeconds(dashDuration);
        dashFlag = false;
    }


    public void Push()
    {
        if(otherPM != null && otherPM != this)
        {
            Vector3 dir = otherPM.transform.position - transform.position;
            dir.y = 0;
            dir = dir.normalized;
            if (otherPM.stunCor != null) StopCoroutine(otherPM.stunCor);
            otherPM.stunCor = otherPM.IsPushed(dir);
            StartCoroutine(otherPM.stunCor);
            StartCoroutine(otherPM.IsPushed(dir));
        }
    }

    public IEnumerator IsPushed(Vector3 pushDirection)
    {
        isStunned = true;
        canMove = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.mass = 1;
        rb.AddForce(pushDirection * pushStrength, ForceMode.Impulse);
        player._characterController.enabled = false;
        yield return new WaitForSeconds(stunDuration);
        UnStun();
    }

    public void UnStun()
    {
        rb.constraints = constraints;
        canMove = true;
        rb.mass = 0;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        rb.isKinematic = true;
        stunCor = null;
        player._characterController.enabled = true;
        isStunned = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement otherPlayerM))
        {
            if(otherPlayerM == this)
            {
                return;
            }
            otherPM = otherPlayerM;            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && otherPM != null && otherPM.gameObject == other.gameObject)
        {
            otherPM = null;
        }
    }
}
