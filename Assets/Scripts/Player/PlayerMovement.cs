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
    [SerializeField] private float dashStrength;
    [SerializeField] public float pushStrength;
    [SerializeField] private float dashDuration;
    [SerializeField] public float dashCooldown = 1;
    [Header("1 = 0.1 sec, .1 = 1 sec")]
    //[SerializeField, Range(0.01f, 1)] private float acceleration = .7f;
    //[SerializeField, Range(0.01f, 1)] private float deceleration = .6f;
    [Space(10)]
    private Vector3 dir;
    private float mvtStr;
    private float speedValue;
    private float dashTimer;
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

    #endregion
    private void Awake()
    {
        /*movingSound = FMODUnity.RuntimeManager.CreateInstance("event:/Tile/Charactere/Moove");
        movingSound.set3DAttributes(new FMOD.ATTRIBUTES_3D());*/
        _characterController = GetComponent<CharacterController>();
        player = GetComponent<Player>();
    }

    private void OnGroundedCallBack()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/stomp");
    }

    private void Update()
    {
        
        if (_characterController.isGrounded && !groundedCallback)
        {
            OnGroundedCallBack();
        }

        groundedCallback = _characterController.isGrounded;
        ApplyGravity();
        ApplyJump();
        ApplyRotation();
        SpeedModifier();
        if (isDashing)
        {
            StartCoroutine(Dash());
            isDashing = false;
        }
        if (dashFlag)
        {
            ApplyDash();
        }
        else
        {
            ApplyMovement();
        }

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

    private void ApplyJump()
    {
        if (_characterController.isGrounded && jumpInput)
        {
            if(player.tileUnder.tileType == Tile.TileType.BouncyTile)
            {
                _velocity = jumpStrengthOnBounce;
            }
            else
            {
                _velocity = jumpStrength;
            }
        }
        jumpInput = false;
    }

    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;

        var targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }

    private void SpeedModifier()
    {
        /*        float ax = isDashing ? acceleration : -deceleration;
                speedValue = Mathf.Lerp(speed, sprintingSpeed, ax * Time.deltaTime * 10);*/

        if(player.tileUnder != null && player.tileUnder.tileType == Tile.TileType.Rock)
        {
            speedValue = speedOnRocks;
        }
        else
        {
            speedValue = speed;
        }
    }

    private void ApplyMovement()
    {
        _characterController.Move(_direction * speedValue * Time.deltaTime);
    }

    private void ApplyDash()
    {
        _characterController.Move(dir * mvtStr * Time.deltaTime) ;
    }
        
        
    private void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        float cameraAngle = -Camera.main.transform.rotation.eulerAngles.y;
        _input = Rotate(_input, cameraAngle);
        _direction = new Vector3(_input.x, 0.0f, _input.y);
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
            player.anim.Play("Jump", 0);
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
}
