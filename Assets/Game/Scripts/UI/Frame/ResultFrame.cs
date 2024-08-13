using TMPro;
using UnityEngine;

public class ResultFrame : MonoBehaviour
{
    [SerializeField]
    Canvas _canvas;

    [SerializeField]
    TextMeshProUGUI _winOrLossText;

    [SerializeField]
    Material _winFontMaterial;

    [SerializeField]
    Material _lossFontMaterial;

    [SerializeField]
    TextMeshProUGUI _selfScoreText;

    [SerializeField]
    Avater _selfAvater;

    [SerializeField]
    TextMeshProUGUI _otherScoreText;

    [SerializeField]
    Avater _otherAvater;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    string _animationName = "Default";

    void Start()
    {
        _canvas.enabled = false;
    }

    // 引数が長くなるのでクラス化
    public void ShowFrame(ShowFrameDescriptor desc)
    {
        _canvas.enabled = true;
        _animator.Play(_animationName, 0, 0);

        _selfScoreText.text = desc.selfScore.ToString();
        _selfAvater.SetData(desc.selfName, desc.selfIcon);

        _otherScoreText.text = desc.otherScore.ToString();
        _otherAvater.SetData(desc.selfName, desc.otherIcon);

        if (desc.isWin)
        {
            _winOrLossText.text = "Win";
            _winOrLossText.fontMaterial = _winFontMaterial;
        } else {
            _winOrLossText.text = "Loss";
            _winOrLossText.fontMaterial = _lossFontMaterial;
        }
    }

    public class ShowFrameDescriptor
    {
        public bool isWin;

        public int selfScore;
        public string selfName;
        public Sprite selfIcon;

        public int otherScore;
        public string otherName;
        public Sprite otherIcon;

        public ShowFrameDescriptor() { }

        public ShowFrameDescriptor(
            bool isWin,
            int selfScore = 0,
            string selfName = "SELF",
            Sprite selfIcon = null,
            int otherScore = 0,
            string otherName = "OTHER",
            Sprite otherIcon = null)
        {
            this.isWin = isWin;

            this.selfScore = selfScore;
            this.selfName = selfName;
            this.selfIcon = selfIcon;

            this.otherScore = otherScore;
            this.otherName = otherName;
            this.otherIcon = otherIcon;
        }
    }
}
