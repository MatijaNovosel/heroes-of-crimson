using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManaBar : MonoBehaviour
{
    public Player player;
    public Image image;
    public Image imageLight;
    public TMP_Text text;
    public TMP_Text regenText;

    private BaseNPCBehaviour baseNPC;

    void Awake()
    {
        if (player)
        {
            baseNPC = player.GetComponent<BaseNPCBehaviour>();
        }
    }

    void Update()
    {
        UpdateFillAmount();
    }
    
    public void UpdateRegenText(float regenPerSecond)
    {
        regenText.text = $"+{regenPerSecond:F2}";
    }

    void UpdateFillAmount()
    {
        if (!baseNPC)
        {
            if (image) image.fillAmount = 0f;
            if (imageLight) imageLight.fillAmount = 0f;
            if (text) text.text = "Dead";
            return;
        }

        var mp = Mathf.Max(0f, baseNPC.mp);
        var maxMp = Mathf.Max(1f, baseNPC.maxMp);
        var fill = Mathf.Clamp01(mp / maxMp);

        if (image) image.fillAmount = fill;
        if (imageLight) imageLight.fillAmount = fill;
        if (text) text.text = $"{Mathf.CeilToInt(mp)}/{Mathf.CeilToInt(maxMp)}";
    }
}