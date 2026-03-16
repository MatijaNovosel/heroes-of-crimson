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

    private BaseNPCBehaviour _baseNPC;

    void Awake()
    {
        if (player) _baseNPC = player.GetComponent<BaseNPCBehaviour>();
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
        if (!_baseNPC)
        {
            image.fillAmount = 0f;
            imageLight.fillAmount = 0f;
            text.text = "Dead";
            regenText.text = "";
            return;
        }

        var mp = Mathf.Max(0f, _baseNPC.mp);
        var maxMp = Mathf.Max(1f, _baseNPC.maxMp);
        var fill = Mathf.Clamp01(mp / maxMp);

        image.fillAmount = fill;
        imageLight.fillAmount = fill;
        text.text = $"{Mathf.CeilToInt(mp)}/{Mathf.CeilToInt(maxMp)}";
    }
}