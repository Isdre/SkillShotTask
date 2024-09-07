using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonShow : MonoBehaviour {
    [SerializeField] private Color32 activeColor;
    [SerializeField] private Color32 disactiveColor;
    [SerializeField] private KeyCode keyCode;
    
    private Image _image;

    private void Awake() {
        _image = GetComponent<Image>();
    }

    private void Update() {
        if (Input.GetKey(keyCode)) _image.color = activeColor;
        else _image.color = disactiveColor;
    }
}
