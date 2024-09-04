using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
  public Player player;
  public Image healthbarImage;
  public TMP_Text healthBarText;
  private BaseNPCBehaviour baseNPCBehaviour;

  void Start()
  {
    baseNPCBehaviour = player.gameObject.GetComponent<BaseNPCBehaviour>();
  }

  void Update()
  {
    //
  }

  public void UpdateFillAmount()
  {
    healthbarImage.fillAmount = Mathf.Clamp(baseNPCBehaviour.hp / baseNPCBehaviour.maxHp, 0, 1f);
    healthBarText.text = $"{baseNPCBehaviour.hp}/{baseNPCBehaviour.maxHp}";
  }
}
