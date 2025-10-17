using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Player player;
    public Image healthbarImage;
    public Image healthbarImageLight;
    public TMP_Text healthBarText;

    private BaseNPCBehaviour baseNPC;

    void Awake()
    {
        if (player != null)
        {
            baseNPC = player.GetComponent<BaseNPCBehaviour>();
        }
    }

    void Update()
    {
        UpdateFillAmount();
    }

    public void UpdateFillAmount()
    {
        if (baseNPC == null)
        {
            if (healthbarImage) healthbarImage.fillAmount = 0f;
            if (healthbarImageLight) healthbarImageLight.fillAmount = 0f;
            if (healthBarText) healthBarText.text = "";
            return;
        }

        var hp = Mathf.Max(0f, baseNPC.hp);
        var maxHp = Mathf.Max(1f, baseNPC.maxHp);
        var fill = Mathf.Clamp01(hp / maxHp);

        if (healthbarImage) healthbarImage.fillAmount = fill;
        if (healthbarImageLight) healthbarImageLight.fillAmount = fill;
        if (healthBarText) healthBarText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}";
    }
}