using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Events;
public class AI_Movement : MonoBehaviour
{
    private AI_Behaviour AI_B;
    private CharacterController _characterController;
    #region Variables: Movement

    [HideInInspector] public Vector2 _input;
    [HideInInspector] public bool jumpInput;
    [HideInInspector] public Vector3 _direction;
    [HideInInspector] public bool moveFlag;
    [HideInInspector] public bool isMoving;

    [SerializeField] private float speed;
    [SerializeField] private float jumpStrength = 10;
    [Header("1 = 0.1 sec, .1 = 1 sec")]
    //[SerializeField, Range(0.01f, 1)] private float acceleration = .7f;
    //[SerializeField, Range(0.01f, 1)] private float deceleration = .6f;
    [Space(10)]
    private float speedValue;
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
    public float rangeToReachDestination = 1;
    public UnityEvent<Transform, AI_Behaviour> onPlayerReached;
    [HideNormalInspector] public Transform Target;
    public LayerMask tileMask;
    #endregion

    private void Awake()
    {
        AI_B = GetComponent<AI_Behaviour>();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnGroundedCallBack()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Stomp", transform.position);
    }

    private void Update()
    {
        if (_characterController.isGrounded && !groundedCallback)
        {
            OnGroundedCallBack();
        }

        switch (AI_B.currentBehavious)
        {
            case AI_Behaviour.Behavious.AI:
                if (AI_B.tilePath.Count > 0)
                {
                    dirInputAI();
                    isMoving = true;
                }
                else goto case AI_Behaviour.Behavious.Static;
                break;
            case AI_Behaviour.Behavious.Target:
                dirInputTarget();
                isMoving = true;
                break;
            case AI_Behaviour.Behavious.Static:
                _direction = Vector3.zero;
                isMoving = false;
                break;  
        }

        groundedCallback = _characterController.isGrounded;
        JumpRayCast();
        ApplyGravity();
        ApplyJump();
        ApplyRotation();
        SpeedModifier();
        ApplyMovement();
    }

    private void dirInputAI()
    {
        if (Vector3.Distance(transform.position , AI_B.tilePath[0].transform.position + 23 * Vector3.up) < rangeToReachDestination)
        {
            AI_B.tilePath.RemoveAt(0);
            if (AI_B.tilePath.Count == 0)
            {
                if(AI_B.target == AI_Behaviour.AITarget.ClosestPlayer)
                {
                    onPlayerReached?.Invoke(AI_B.targetTile.transform, AI_B);
                }
                _direction = Vector3.zero;
                return;
            }
        }
        _input = new Vector2((AI_B.tilePath[0].transform.position - transform.position).x, (AI_B.tilePath[0].transform.position - transform.position).z).normalized;
        _direction = new Vector3(_input.x, 0.0f, _input.y);
        
    }

    private void dirInputTarget()
    {
        _input = new Vector2((Target.position - transform.position).x, (Target.position - transform.position).z).normalized;
        _direction = new Vector3(_input.x, 0.0f, _input.y);
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
        /*        float ax = isDashing ? acceleration : -deceleration;
                speedValue = Mathf.Lerp(speed, sprintingSpeed, ax * Time.deltaTime * 10);*/
        speedValue = speed;
    }

    private void ApplyMovement()
    {
        if(_characterController.enabled)
        _characterController.Move(_direction * speedValue * Time.deltaTime);
    }

    private void JumpRayCast()
    {
        if(Physics.RaycastAll(transform.position, transform.forward + transform.localScale.y / 2 * Vector3.up, 1f, tileMask, QueryTriggerInteraction.Ignore).Length > 0 && _characterController.isGrounded)
        {
            jumpInput = true;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.TryGetComponent<Tile>(out Tile tileO) && hit.normal.y > -0.2f && hit.normal.y < 0.2f && hit.transform.position.y - AI_B.tileUnder.transform.position.y <= 3 && hit.transform.position.y - AI_B.tileUnder.transform.position.y > 1)
        {
            jumpInput = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position + transform.localScale.y / 2 * Vector3.up, transform.forward * 1f);
    }
}
