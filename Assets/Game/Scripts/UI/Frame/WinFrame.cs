using System.Collections;
using UnityEngine;

public class WinFrame : MonoBehaviour
{
    [SerializeField]
    float _cooldownTime = 2;

    [SerializeField]
    Canvas _canvas;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    string _animationName = "Default";

    Coroutine _coroutine;

    void Start()
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
