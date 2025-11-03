using System;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseNPCBehaviour : MonoBehaviour
{
  // Public stats
  public float hp = 100;
  public float mp = 100;
  public float maxMp = 100;
  public float maxHp = 100;
  
  public float spd = 30;
  public float att = 30;
  public float def = 0;
  public float vit = 30;
  public float wis = 30;
  public float dex = 30;
  
  public bool invincible = false;
  public List<Constants.StatusEffects> statusEffects = new () { Constants.StatusEffects.ArmorBroken, Constants.StatusEffects.Bleeding };

  // Immunity
  private const float ImmuneTime = 1.0f;
  private float _lastImmune;

  // Other
  public AudioClip deathSound;
  public AudioClip hitSound;
  
  // UI stuff
  private GameObject _statusEffectPanel;

  private void Awake()
  {
    var statusEffectPanelPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectPanel");
    _statusEffectPanel = Instantiate(
      statusEffectPanelPrefab,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      Quaternion.identity
    );
    _statusEffectPanel.GetComponent<StatusEffectPanel>().Setup(statusEffects, gameObject);
  }

  private void Die()
  {
    if (deathSound)
    {
      AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    }

    Destroy(gameObject);
  }

  private void DisplayStatusEffects()
  {
    if (statusEffects.Count == 0) return;
    _statusEffectPanel.GetComponent<StatusEffectPanel>().SetStatusEffects(statusEffects);
  }

  private void Update()
  {
    DisplayStatusEffects();
  }

  private void ReceiveDamage(DamageModel payload)
  {
    if (hitSound)
    {
      AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.5f);
    }
    
    var angle = Random.Range(-40f, 40f);
    var radians = angle * Mathf.Deg2Rad;
    var randomDirection = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);

    var randomRotation = Random.Range(-15f, 15f);

    var minDamage = 1f;
    var maxDamage = 100f;
    var minFontSize = 100f;
    var maxFontSize = 250f;

    var t = Mathf.InverseLerp(minDamage, maxDamage, payload.Value);
    var fontSize = Mathf.Lerp(minFontSize, maxFontSize, t);
    var randomScale = Random.Range(0.9f, 1.2f);

    var textObj = GameManager.Instance.ShowText(
      $"-{payload.Value}",
      (int)fontSize,
      Color.red,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      randomDirection,
      2.0f
    );

    if (textObj != null)
    {
      textObj.obj.transform.rotation = Quaternion.Euler(0, 0, randomRotation);
      textObj.obj.transform.localScale *= randomScale;
    }

    if (!(Time.time - _lastImmune > ImmuneTime) || invincible) return;

    hp -= payload.Value;

    if (hp <= 0)
    {
      hp = 0;
      Die();
    }
  }
}