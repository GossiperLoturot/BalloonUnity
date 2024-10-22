﻿using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class DialogFrame : MonoBehaviour
{
    [SerializeField]
    float _cooldownTime = 1;

    [SerializeField]
    Canvas _canvas;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    string _animationName = "Default";

    Coroutine _coroutine;

    void Awake()
    {
        _canvas.enabled = false;
    }

    public void ShowFrame()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(ShowFrameCoroutine());
    }

    IEnumerator ShowFrameCoroutine()
    {
        _canvas.enabled = true;
        _animator.Play(_animationName, 0, 0);

        yield return new WaitForSeconds(_cooldownTime);
        
        _canvas.enabled = false;
    }
}
