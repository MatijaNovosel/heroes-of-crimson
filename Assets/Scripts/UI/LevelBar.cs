using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    public Player player;
    public Image image;
    public TMP_Text text;
    int xpNeeded = 100;
    
    void Update()
    {
        UpdateFillAmount();
    }

    void UpdateFillAmount()
    {
        if (!player)
        {
            if (image) image.fillAmount = 0f;
            if (text) text.text = "1";
            return;
        }

        var xp = player.experience;
        float fill = Mathf.Clamp01((float)xp / xpNeeded);

        if (image) image.fillAmount = fill;
        if (text) text.text = player.level.ToString();
    }
}
