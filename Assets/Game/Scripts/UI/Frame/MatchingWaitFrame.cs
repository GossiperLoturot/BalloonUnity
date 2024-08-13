using System.Collections;
using UnityEngine;

public class MatchingWaitFrame : MonoBehaviour
{
    [SerializeField]
    Canvas _canvas;

    public void ShowFrame()
    {
        _canvas.enabled = true;
    }

    public void HideFrame()
    {
        _canvas.enabled = false;
    }

}
