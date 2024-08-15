using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CompatView : MonoBehaviour
{
    [SerializeField]
    Canvas _canvas;

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
}
