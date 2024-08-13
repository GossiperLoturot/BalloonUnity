using UnityEngine;
using TMPro;
using System;

public class HUDFrame : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _selfScore;

    [SerializeField]
    TextMeshProUGUI _otherScore;

    [SerializeField]
    TextMeshProUGUI _remainTime;


    public void SetSelfScore(int score)
    {
        _selfScore.text = score.ToString();
    }

    public void SetOtherScore(int score)
    {
        _otherScore.text = score.ToString();
    }

    public void SetRemainTime(float remainTime)
    {
        var span = new TimeSpan(0, 0, (int)remainTime);
        _remainTime.text = span.ToString(@"mm\:ss");
    }
}
