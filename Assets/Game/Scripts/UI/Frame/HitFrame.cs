using System.Collections;
using UnityEngine;

public class HitFrame : MonoBehaviour
{
    [SerializeField]
    float _cooldownTime = 2;

    [SerializeField]
    ParticleSystem[] _particles;

    [SerializeField]
    Avater _avatar;

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

    public void ShowFrame(string name, Sprite icon)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(ShowFrameCoroutine(name, icon));
    }

    IEnumerator ShowFrameCoroutine(string name, Sprite icon)
    {
        _canvas.enabled = true;
        _animator.Play(_animationName, 0, 0);

        foreach (var particle in _particles) particle.Play();

        _avatar.SetData(name, icon);

        yield return new WaitForSeconds(_cooldownTime);
        _canvas.enabled = false;
    }
}
