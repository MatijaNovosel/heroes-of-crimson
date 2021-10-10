using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
  public Player player;
  public Image healthbarImage;
  private BaseNPCBehaviour baseNPCBehaviour;

  void Start()
  {
    baseNPCBehaviour = player.gameObject.GetComponent<BaseNPCBehaviour>();
  }

  void Update()
  {
    //
  }

  public void UpdateHealthbar()
  {
    healthbarImage.fillAmount = Mathf.Clamp(baseNPCBehaviour.hp / baseNPCBehaviour.maxHp, 0, 1f);
  }
}
