using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ResultFrame : MonoBehaviour
{
    [SerializeField]
    Canvas _canvas;

    [SerializeField]
    TextMeshProUGUI _text;

    [SerializeField]
    Material _winFontMaterial;

    [SerializeField]
    Material _drawFontMaterial;

    [SerializeField]
    Material _loseFontMaterial;

    [SerializeField]
    TextMeshProUGUI _selfScoreText;

    [SerializeField]
    TextMeshProUGUI _otherScoreText;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    string _animationName = "Default";

    void Awake()
    {
        _canvas.enabled = false;
    }

    // 引数が長くなるのでクラス化
    public void ShowFrame(int selfScore, int otherScore)
    {
        _canvas.enabled = true;
        _animator.Play(_animationName, 0, 0);

        _selfScoreText.text = selfScore.ToString();

        _otherScoreText.text = otherScore.ToString();

        if (selfScore > otherScore)
        {
            _text.text = "Win";
            _text.fontMaterial = _winFontMaterial;
        }
        else if (otherScore > selfScore)
        {
            _text.text = "Lose";
            _text.fontMaterial = _loseFontMaterial;
        }
        else
        {
            _text.text = "Draw";
            _text.fontMaterial = _drawFontMaterial;
        }
    }
}
