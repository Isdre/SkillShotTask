using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody _rigidbody;
    private Transform _transform;
    
    [Header("Speed Settings")]
    [SerializeField] private float speed = 25f;
    private float _speedConstraint = 20f;
    [SerializeField] private float groundSpeedConstraint = 20f;
    [SerializeField] private float airSpeedConstraint = 30f;

    private Vector3 _direction;

    private float horizontalInput;
    private float verticalInput;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float airControler = 0.25f;
    [SerializeField] private float airDrag;
    [SerializeField] private float jumpDelay = 0.25f;
    private float _jumpDelayTimer = 0f;

    [Header("Dash Settings")] 
    [SerializeField] private float dashForce;
    [SerializeField] private float dashCooldown;
    private float _dashTimer;

    [Header("Slide Settings")] 
    [SerializeField] private float slideDrag;
    [SerializeField] private float smooth = 5.0f;
    [SerializeField] private float tiltAngle = 60.0f;
    
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    private float _slideTimer;
    private float startYScale = 1f;
    
    private Quaternion _targetRotation;
    private bool _isSliding;
    
    [Header("Ground Settings")]
    [SerializeField] private float groundDrag;
    [SerializeField] private LayerMask groundLayer;
    private bool _grounded;
    
    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        _transform = transform;
    }
    
    private void Update() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        _grounded = Physics.Raycast(_transform.position, Vector3.down, startYScale + 0.2f, groundLayer);

        if (_grounded) {
            if (_isSliding) _rigidbody.drag = slideDrag;
            else _rigidbody.drag = groundDrag;
        }
        else _rigidbody.drag = airDrag;
        
        SpeedConstraint();

        if (Input.GetKeyDown(KeyCode.Space)) _jumpDelayTimer = jumpDelay;
        _jumpDelayTimer -= Time.deltaTime;
        
        if (_grounded) {
            if (_jumpDelayTimer > 0f) {
                _speedConstraint = airSpeedConstraint;
                _jumpDelayTimer = 0f;
                Jump();
            }
            else _speedConstraint = groundSpeedConstraint;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) & (_direction.x != 0 || _direction.z != 0))
            StartSlide();
        
        if (Input.GetKeyUp(KeyCode.LeftControl) & _isSliding)
            StopSlide();
        
        _transform.rotation = Quaternion.Slerp(_transform.rotation, _targetRotation,  Time.deltaTime * smooth);
    }

    private void FixedUpdate() {
        SetDirection();
        if (_isSliding) {
            Sliding();
        }
        else {
            Move();
        }
    }

    private void SetDirection() {
        _direction = new Vector3(horizontalInput, 0f, verticalInput);
    }
    
    private void Move() {
        _targetRotation = Quaternion.identity;
        if (_grounded) _rigidbody.AddForce(_direction * speed, ForceMode.Force);
        else _rigidbody.AddForce(speed * airControler * _direction, ForceMode.Force);
    }

    private void SpeedConstraint() {
        Vector3 speedVector = _rigidbody.velocity;
        speedVector.y = 0f;

        if (speedVector.magnitude > _speedConstraint) {
            Vector3 limitedSpeed = speedVector.normalized * _speedConstraint;
            _rigidbody.velocity = limitedSpeed;
        }
    }

    private void Jump() {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        _rigidbody.AddForce(_transform.up * jumpForce, ForceMode.Impulse);
    }

    private void Dash() {
        
    }

    private void Sliding() {
        Vector3 slope = new Vector3(-1 * _direction.normalized.z,0f,_direction.normalized.x);
        slope *= tiltAngle;
        _targetRotation = Quaternion.Euler(slope);
        
        _rigidbody.AddForce(_direction * slideForce, ForceMode.Force);

        _slideTimer -= Time.deltaTime;
        
        if (_slideTimer <= 0f) StopSlide();
    }

    private void StartSlide() {
        _isSliding = true;
        _slideTimer = maxSlideTime;
        Vector3 slide = new Vector3(_direction.x, -.5f, _direction.z);
        _rigidbody.AddForce(slideForce * slide, ForceMode.Impulse);
    }
    
    private void StopSlide() {
        _isSliding = false;
    }
}
