using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController _characterController;
    [Header("Player")]
    [Tooltip("Base movement speed of the player")]
    [SerializeField]
    private float _movementSpeed;
    [SerializeField]
    private float _sprintSpeed;
    [SerializeField]
    private float _turnSmoothTime = .1f;
    [SerializeField]
    private Transform _cam;
    [SerializeField]
    private float _jumpHeight;
    [SerializeField]
    [Tooltip("Controls how soon the palyer can jump after touching the ground.")]
    private float _jumpTimeout = .5f;
    [SerializeField]
    private float _gravity;

    //Camera Settings
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField]
    private GameObject _cinemachineFollowTarget;
    [SerializeField]
    [Tooltip("How far in degrees can you move the camera up")]
    private float _topClamp = 70.0f;
    [SerializeField]
    [Tooltip("How far in degrees can you move the camera down")]
    private float _bottomClamp = -30.0f;
    [SerializeField]
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    private float _cameraAngleOverride = 0.0f;
    private GameObject _testObject;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshhold = .01f;
    private Camera _mainCam;

    private PlayerInputActions _playerInputActions;
    private InputAction _movement;
    private InputAction _look;

    //Player
    private float _jumpTimeoutDelta;
    private float _turnSmoothVelocity;
    private float _targetRotation = 0.0f;
    private float _terminalVelocity = 53.0f;
    private bool _sprinting = false;
    private bool _jumping = false;
    private bool _aiming = false;

    //Input
    private Vector3 _direction;
    private Vector3 _moveDirection;
    private Vector3 _jumpVector;
    private float _verticalVelocity;
    private float _targetSpeed;


    //Player Driven Delegates
    public static Action PlayerInteractAction;
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _mainCam = Camera.main;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _cinemachineTargetYaw = _cinemachineFollowTarget.transform.rotation.eulerAngles.y;
    }

    private void OnEnable()
    {
        _movement = _playerInputActions.Player.Movement;
        _look = _playerInputActions.Player.Look;

        _playerInputActions.Player.Jump.performed += OnJump;
        _playerInputActions.Player.Sprint.started += OnSprint;
        _playerInputActions.Player.Sprint.canceled += OnSprint;
        _playerInputActions.Player.Interact.performed += OnInteract;
        _playerInputActions.Player.Attack.performed += OnAttack;
        _playerInputActions.Player.Aim.started += OnAim;
        _playerInputActions.Player.Aim.canceled += OnAim;
        _playerInputActions.Player.Enable();
    }

    private void OnAttack(InputAction.CallbackContext obj)
    {
        UnityEngine.Debug.Log("I am Attacking!");
    }

    private void OnInteract(InputAction.CallbackContext obj)
    {
        PlayerInteractAction?.Invoke();
    }

    private void OnSprint(InputAction.CallbackContext obj)
    {
        _sprinting = !_sprinting;
    }

    private void OnAim(InputAction.CallbackContext obj)
    {
        _aiming = !_aiming;
        UnityEngine.Debug.Log("AIMING");
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        _jumping = true;
    }

    private void Update()
    { 
        ReadMovement();
        JumpAndGravity();
    }
    private void FixedUpdate()
    {
        ApplyMovement();
        CameraRotation();
    }
    private void JumpAndGravity()
    {
        if(_characterController.isGrounded)
        {
            //prevent infinite dropping of _verticalVelocity
            if(_verticalVelocity < .0f)
            {
                _verticalVelocity = -2f;
            }
            if(_jumping && _jumpTimeoutDelta <= .0f)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * 2f * _gravity);
            }
            if(_jumpTimeoutDelta >= .0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = _jumpTimeout;

            //Apply gravity over time if not at terminal velocity
            if(_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity -= _gravity * Time.deltaTime;
            }
            _jumping = false;
        }
    }

    private void ApplyMovement()
    {
        //If sprinting change movement speed
        //float targetSpeed = _sprinting ? _sprintSpeed : _movementSpeed;
        //if (_movement.ReadValue<Vector2>() == Vector2.zero) targetSpeed = 0f;
        //Get the x and y values from player input.
        //We normalize so that a diagonal input does not create faster movement
        //_direction = new Vector3(_movement.ReadValue<Vector2>().x, 0f, _movement.ReadValue<Vector2>().y).normalized;
        //_moveDirection = new Vector3();
        //Vector3 jumpVector;
            //If moving and not aiming rotate towards camera direction
            if (_movement.ReadValue<Vector2>() != Vector2.zero && !_aiming)
            {
                //Get angle you want to rotate to
                _targetRotation = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + _mainCam.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _turnSmoothVelocity, _turnSmoothTime);

                transform.rotation = Quaternion.Euler(0f, rotation, 0f);

            }
        _moveDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
        _jumpVector = new Vector3(0.0f, _verticalVelocity, 0.0f);
        _characterController.Move(_moveDirection.normalized * (_targetSpeed * Time.fixedDeltaTime) + (_jumpVector * Time.fixedDeltaTime));
    }

    private void ReadMovement()
    {
        _targetSpeed = _sprinting ? _sprintSpeed : _movementSpeed;
        if (_movement.ReadValue<Vector2>() == Vector2.zero) _targetSpeed = 0f;
        //Read the X and Y input from the player; normalize them; translate them to X and Z values;
        _direction = new Vector3(_movement.ReadValue<Vector2>().x, 0f, _movement.ReadValue<Vector2>().y).normalized;
    }
    private void CameraRotation()
    {
        if( _look.ReadValue<Vector2>().sqrMagnitude >= _threshhold)
        {
            _cinemachineTargetYaw += _look.ReadValue<Vector2>().x * Time.fixedDeltaTime;
            _cinemachineTargetPitch += _look.ReadValue<Vector2>().y * Time.fixedDeltaTime;
        }
            //Clamp angles to 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);
            _cinemachineFollowTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, .0f);      
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= OnJump;
        _playerInputActions.Player.Sprint.started -= OnSprint;
        _playerInputActions.Player.Sprint.canceled -= OnSprint;
        _playerInputActions.Player.Aim.started -= OnAim;
        _playerInputActions.Player.Aim.canceled -= OnAim;
        _playerInputActions.Player.Interact.performed -= OnInteract;
        _playerInputActions.Player.Attack.performed -= OnAttack;
        _playerInputActions.Player.Disable();
    }

}
