using System;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-100)]
public class ReadyFrame : MonoBehaviour
{
    [SerializeField]
    Canvas _canvas;

    [SerializeField]
    TMP_Text _remainTime;

    void Awake()
    {
        _canvas.enabled = false;
    }

    public void ShowFrame()
    {
        _canvas.enabled = true;
    }

    public void HideFrame()
    {
        _canvas.enabled = false;
    }

    public void SetRemainTime(float remainTime)
    {
        var span = new TimeSpan(0, 0, (int)remainTime);
        _remainTime.text = span.ToString(@"ss");
    }
}
