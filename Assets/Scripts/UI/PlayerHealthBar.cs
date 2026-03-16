using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    public Player player;
    public Image healthbarImage;
    public Image healthbarImageLight;
    public TMP_Text healthBarText;
    public TMP_Text regenText;

    private BaseNPCBehaviour _baseNPC;

    void Awake()
    {
        if (player) _baseNPC = player.GetComponent<BaseNPCBehaviour>();
    }

    void Update()
    {
        _updateFillAmount();
    }

    public void UpdateRegenText(float regenPerSecond)
    {
        regenText.text = $"+{regenPerSecond:F2}";
    }

    private void _updateFillAmount()
    {
        if (!_baseNPC)
        {
            healthbarImage.fillAmount = 0f;
            healthbarImageLight.fillAmount = 0f;
            healthBarText.text = "Dead";
            healthBarText.text = "Dead";
            regenText.text = "";
            return;
        }

        var hp = Mathf.Max(0f, _baseNPC.hp);
        var maxHp = Mathf.Max(1f, _baseNPC.maxHp);
        var fill = Mathf.Clamp01(hp / maxHp);

        healthbarImage.fillAmount = fill;
        healthbarImageLight.fillAmount = fill;
        healthBarText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}";
    }
}