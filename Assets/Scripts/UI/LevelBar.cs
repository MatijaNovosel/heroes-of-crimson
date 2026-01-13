using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    public Player player;
    private Image _image;
    public TMP_Text text;

    void Start()
    {
        _image = GetComponent<Image>();
    }
    
    void Update()
    {
        UpdateFillAmount();
    }

    void UpdateFillAmount()
    {
        if (!player)
        {
            if (_image) _image.fillAmount = 0f;
            if (text) text.text = "1";
            return;
        }

        var xp = player.Experience;
        var xpNeeded = 100;
        var fill = Mathf.Clamp01(xp / xpNeeded);

        if (_image) _image.fillAmount = fill;
        if (text) text.text = player.Level.ToString();
    }
}
