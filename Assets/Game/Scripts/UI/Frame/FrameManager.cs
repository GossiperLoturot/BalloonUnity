using UnityEngine;

// 適当に作成したUIの集約管理用クラス
public class FrameManager : MonoBehaviour
{
    [SerializeField]
    HUDFrame _hudFrame;

    [SerializeField]
    HitFrame _selfHitFrame;

    [SerializeField]
    HitFrame _otherHitFrame;

    [SerializeField]
    WinFrame _winFrame;

    [SerializeField]
    LoseFrame _loseFrame;

    [SerializeField]
    ResultFrame _resultFrame;

    // 描写用のキャッシュ値
    int _selfScore;
    int _otherScore;
    float _remainTime;
    bool _isFinished;

    public void SetSelfScore(int score)
    {
        _hudFrame.SetSelfScore(score);
        if (score > _selfScore) _selfHitFrame.ShowFrame("SELF", null);
        _selfScore = score;
    }

    public void SetOtherScore(int score)
    {
        _hudFrame.SetOtherScore(score);
        if (score > _otherScore) _otherHitFrame.ShowFrame("OTHER", null);
        _otherScore = score;
    }

    public void SetRemainTime(float remainTime)
    {
        _hudFrame.SetRemainTime(remainTime);
        _remainTime = remainTime;

        if (remainTime <= 0 && !_isFinished)
        {
            var isWin = _selfScore > _otherScore;
            if (isWin)
            {
                _winFrame.ShowFrame();
            }
            else
            {
                _loseFrame.ShowFrame();
            }
            _resultFrame.ShowFrame(new ResultFrame.ShowFrameDescriptor(
                isWin,
                _selfScore, "SELF", null,
                _otherScore, "OTHER", null
            ));
            _isFinished = true;
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUI.Button(new Rect(600, 0, 200, 40), "Add Self Score"))
        {
            SetSelfScore(_selfScore + 1);
        }

        if (GUI.Button(new Rect(600, 50, 200, 40), "Add Other Score"))
        {
            SetOtherScore(_otherScore + 1);
        }

        if (GUI.Button(new Rect(600, 100, 200, 40), "Start Time"))
        {
            SetRemainTime(60);
        }

        if (GUI.Button(new Rect(600, 150, 200, 40), "Elapsed Time"))
        {
            SetRemainTime(_remainTime - 10);
        }

        if (GUI.Button(new Rect(400, 0, 200, 40), "Show Self Hit Frame"))
        {
            _selfHitFrame.ShowFrame("Self", null);
        }

        if (GUI.Button(new Rect(400, 50, 200, 40), "Show Other Hit Frame"))
        {
            _otherHitFrame.ShowFrame("Other", null);
        }

        if (GUI.Button(new Rect(400, 150, 200, 40), "Show Win Frame"))
        {
            _winFrame.ShowFrame();
        }

        if (GUI.Button(new Rect(400, 200, 200, 40), "Show Lose Frame"))
        {
            _loseFrame.ShowFrame();
        }

        if (GUI.Button(new Rect(400, 250, 200, 40), "Show Result Frame"))
        {
            _resultFrame.ShowFrame(new ResultFrame.ShowFrameDescriptor(false));
        }
    }
#endif
}