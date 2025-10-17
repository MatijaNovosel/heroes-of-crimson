using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
  public Player player;
  public Image healthbarImage;
  public Image healthbarImageLight;
  public TMP_Text healthBarText;
  private BaseNPCBehaviour BaseNPCBehaviour;

  void Awake()
  {
    BaseNPCBehaviour = player.gameObject.GetComponent<BaseNPCBehaviour>();
  }

  public void UpdateFillAmount()
  {
    healthbarImage.fillAmount = Mathf.Clamp(BaseNPCBehaviour.hp / BaseNPCBehaviour.maxHp, 0, 1f);
    healthbarImageLight.fillAmount = Mathf.Clamp(BaseNPCBehaviour.hp / BaseNPCBehaviour.maxHp, 0, 1f);
    healthBarText.text = $"{BaseNPCBehaviour.hp}/{BaseNPCBehaviour.maxHp}";
  }
}
