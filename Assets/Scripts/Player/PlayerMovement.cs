using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;

public class PlayerMovement : MonoBehaviour
{
    #region Variables: Movement
    public Tile respawnTile;
    private Vector2 _input;
    private bool jumpInput;
    private CharacterController _characterController;
    private Vector3 _direction;
    [HideInInspector] public EventInstance movingSound;
    private TileSelector tileSelec;
    [SerializeField] private float speed;
    [SerializeField] private float jumpStrength = 10;
    [SerializeField] private float sprintingSpeed;
    [Header("1 = 0.1 sec, .1 = 1 sec")]
    [SerializeField, Range(0.01f, 1)] private float acceleration = .7f;
    [SerializeField, Range(0.01f, 1)] private float deceleration = .6f;
    [Space(10)]
    private float speedValue;
    bool isSprinting;
    bool moveFlag;
    #endregion
    #region Variables: Rotation

    [SerializeField] private float smoothTime = 0.05f;
    private float _currentVelocity;

    #endregion
    #region Variables: Gravity
    private bool groundedCallback;
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    [HideInInspector] public float _velocity;

    #endregion

    private void Awake()
    {
/*        movingSound = FMODUnity.RuntimeManager.CreateInstance("event:/Tile/Charactere/moov");
        movingSound.set3DAttributes(new FMOD.ATTRIBUTES_3D());*/
        _characterController = GetComponent<CharacterController>();
        tileSelec = GetComponent<TileSelector>();
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
        ApplyMovement();
        if (_characterController.isGrounded && _input != Vector2.zero && !moveFlag)
        {
            moveFlag = true;
            movingSound.start();
        }
        else if ((!_characterController.isGrounded || _input == Vector2.zero) && moveFlag)
        {
            moveFlag = false;
            movingSound.stop(STOP_MODE.ALLOWFADEOUT);
        }
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
            _velocity = jumpStrength;
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
        float ax = isSprinting ? acceleration : -deceleration;
        speedValue = Mathf.Lerp(speed, sprintingSpeed, ax * Time.deltaTime * 10);
    }

    private void ApplyMovement()
    {
        _characterController.Move(_direction * speedValue * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context)
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

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isSprinting = true;
        }
        else if (context.canceled || context.performed)
        {
            isSprinting = false;
        }
    }

    private void OnDestroy()
    {
        movingSound.stop(STOP_MODE.IMMEDIATE);
        movingSound.release();
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.TryGetComponent<Tile>(out Tile tileO)&& hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - tileSelec.tileUnder.transform.position.y <= 3 && hit.transform.position.y - tileSelec.tileUnder.transform.position.y > 1)
        {
            jumpInput = true;
        }
        else if (hit.transform.CompareTag("Water"))
        {
            transform.position = respawnTile.transform.position + 25f * Vector3.up;
        }
    }
}
