using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Avater : MonoBehaviour
{
    public Image _image;
    public TextMeshProUGUI _nameText;

    public void SetData(string name, Sprite icon)
    {
        _nameText.text = name;
        _image.sprite = icon;
    }
}
