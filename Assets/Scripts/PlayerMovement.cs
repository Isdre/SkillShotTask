using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    private Rigidbody _rigidbody;
    private Transform _transform;
    [SerializeField] private TMP_Text text;
    
    
    [Header("Speed Settings")]
    [SerializeField] private float speed = 25f;
    private float _speedConstraint;
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
    [SerializeField] private float dashDelay = 0.25f;
    private float _dashTimer;
    private float _dashDelayTimer;

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

        _speedConstraint = groundSpeedConstraint;
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

        if (Input.GetKeyDown(KeyCode.Space)) _jumpDelayTimer = jumpDelay;
        _jumpDelayTimer -= Time.deltaTime;
        
        if (_grounded && _jumpDelayTimer > 0f) {
            if (jumpDelay - _jumpDelayTimer < 0.15f) _dashTimer = 0f;
            _jumpDelayTimer = 0f;
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) & (_direction.x != 0 || _direction.z != 0))
            StartSlide();
        
        if (Input.GetKeyUp(KeyCode.LeftControl) & _isSliding)
            StopSlide();

        _dashTimer -= Time.deltaTime;
        _dashDelayTimer -= Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.LeftShift)) _dashDelayTimer = dashDelay;
        
        if (_dashDelayTimer >= 0f & _dashTimer <= 0f) Dash();
        
        
        SpeedConstraint();
        
        _transform.rotation = Quaternion.Slerp(_transform.rotation, _targetRotation,  Time.deltaTime * smooth);
        
        Vector3 xzSpeed = _rigidbody.velocity;
        xzSpeed.y = 0f;
        
        text.text = "Speed: " + xzSpeed.magnitude.ToString();
        Debug.Log(_speedConstraint);
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
        if (_grounded) _rigidbody.AddForce(_direction.normalized * speed, ForceMode.Force);
        else _rigidbody.AddForce(speed * airControler * _direction.normalized, ForceMode.Force);
    }

    private void SpeedConstraint() {
        if (_grounded & _dashTimer <= 0f) _speedConstraint = groundSpeedConstraint;
        
        Vector3 speedVector = new Vector3(_rigidbody.velocity.x,0f,_rigidbody.velocity.z);

        if (speedVector.magnitude > _speedConstraint) {
            Vector3 limitedSpeed = speedVector.normalized * _speedConstraint;
            _rigidbody.velocity = new Vector3(limitedSpeed.x, _rigidbody.velocity.y, limitedSpeed.z);
        }
    }

    private void Jump() {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        _rigidbody.AddForce(new Vector3(0f,jumpForce,0f), ForceMode.Impulse);
    }

    private void Dash() {
        _speedConstraint = airSpeedConstraint;
        _rigidbody.AddForce(dashForce * _direction.normalized, ForceMode.Impulse);
        _dashTimer = dashCooldown;
    }

    private void Sliding() {
        Vector3 slope = new Vector3(-1 * _direction.normalized.z,0f,_direction.normalized.x);
        slope *= tiltAngle;
        _targetRotation = Quaternion.Euler(slope);

        Vector3 slide = _direction.normalized * slideForce;
        slide.y = _rigidbody.velocity.y;
        
        _rigidbody.AddForce(slide, ForceMode.Force);

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
