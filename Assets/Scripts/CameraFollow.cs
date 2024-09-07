using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    private Transform _transform;

    private void Start() {
        _transform = transform;
    }

    private void Update() {
        _transform.position = player.position + offset;
    }
}
