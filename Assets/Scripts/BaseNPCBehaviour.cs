using System;
using System.Collections.Generic;
using System.Linq;
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

  public int xpValue = 15;
  
  public float swf = 30;
  public float mgt = 30;
  public float arm = 0;
  public float str = 30;
  public float wis = 30;
  public float agi = 30;
  
  public bool invincible = false;
  public List<ActiveStatusEffect> ActiveStatusEffects = new();
  private BoxCollider2D _boxCollider;
  
  public int[] lootIds;

  // Other
  public AudioClip deathSound;
  public AudioClip hitSound;
  public GameObject lootBagPrefab;
  
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
    _statusEffectPanel.GetComponent<StatusEffectPanel>().Setup(
      ActiveStatusEffects.Select(x => x.Type).ToList(),
      gameObject
    );
    _boxCollider = GetComponent<BoxCollider2D>();
  }

  private void Die()
  {
    if (deathSound)
    {
      AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    }
    
    var lootBag = Instantiate(
      lootBagPrefab,
      transform.position,
      Quaternion.identity
    );

    lootBag.GetComponent<LootBag>().GenerateLoot(lootIds, true);
    AudioManager.Singleton.PlaySoundCached(Constants.Sounds.LootDrop);
    Player.Singleton.GiveXp(xpValue);

    Destroy(gameObject);
  }
  
  public void Move(Vector2 direction)
  {
    if (_boxCollider == null) return;

    direction = direction.normalized;

    var speed = Utils.CalculatePlayerMovementSpeed(swf);
    LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

    var moveY = new Vector2(0, direction.y);

    if (moveY.y != 0 &&
        !Physics2D.BoxCast(
          transform.position,
          _boxCollider.size,
          0,
          moveY,
          Mathf.Abs(moveY.y * speed),
          mask)
        )
    {
      transform.Translate(0, moveY.y * speed, 0);
    }

    var moveX = new Vector2(direction.x, 0);

    if (moveX.x != 0 &&
        !Physics2D.BoxCast(
          transform.position,
          _boxCollider.size,
          0,
          moveX,
          Mathf.Abs(moveX.x * speed),
          mask)
        )
    {
      transform.Translate(moveX.x * speed, 0, 0);
    }
  }
  
  private void DisplayStatusEffects()
  {
    if (ActiveStatusEffects.Count == 0)
    {
      _statusEffectPanel.GetComponent<StatusEffectPanel>().SetStatusEffects(new ());
      return;
    }

    var effects = ActiveStatusEffects.Select(e => e.Type).ToList();
    _statusEffectPanel.GetComponent<StatusEffectPanel>().SetStatusEffects(effects);
  }
  
  private void UpdateStatusEffects()
  {
    float now = Time.time;

    for (int i = ActiveStatusEffects.Count - 1; i >= 0; i--)
    {
      var effect = ActiveStatusEffects[i];

      switch (effect.Type)
      {
        case Constants.StatusEffects.Bleeding:
          //
          break;
        case Constants.StatusEffects.ArmorBroken:
          arm = 0;
          break;
      }

      if (effect.ExpireTime <= now)
      {
        ActiveStatusEffects.RemoveAt(i);
      }
    }
  }

  private void Update()
  {
    UpdateStatusEffects();
    DisplayStatusEffects();
  }

  public void ApplyStatusEffect(Constants.StatusEffects effect, float duration = 5f)
  {
    var existing = ActiveStatusEffects.FirstOrDefault(e => e.Type == effect);

    if (existing != null)
    {
      // Refresh duration instead of stacking duplicates
      existing.ExpireTime = Time.time + duration;
    }
    else
    {
      ActiveStatusEffects.Add(new ActiveStatusEffect(effect, duration));
      var data = Utils.GetStatusEffectData(effect);
      
      GameManager.Singleton.ShowText(
        data.Name,
        250,
        data.IsNegative ? Color.red: Color.green,
        new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
        Vector3.up,
        2.0f
      );
    }
  }
  
  private float CalculateDamageAfterDefense(float incomingDamage)
  {
    if (incomingDamage <= 0) return 0;

    float reducedByDef = incomingDamage - arm;
    float minAllowedDamage = incomingDamage * 0.1f; // 10% always goes through

    return Mathf.Max(reducedByDef, minAllowedDamage);
  }
  
  private void ReceiveDamage(DamageModel payload)
  {
    if (hitSound) AudioManager.Singleton.PlaySound(hitSound);

    foreach (var effect in payload.StatusEffects)
    {
      ApplyStatusEffect(effect);
    }

    if (invincible) return;

    float finalDamage = CalculateDamageAfterDefense(payload.Value);

    hp -= finalDamage;

    var angle = Random.Range(-40f, 40f);
    var radians = angle * Mathf.Deg2Rad;
    var randomDirection = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);
    var randomRotation = Random.Range(-15f, 15f);

    var minDamage = 1f;
    var maxDamage = 100f;
    var minFontSize = 200f;
    var maxFontSize = 280f;

    var t = Mathf.InverseLerp(minDamage, maxDamage, finalDamage);
    var fontSize = Mathf.Lerp(minFontSize, maxFontSize, t);
    var randomScale = Random.Range(0.9f, 1.2f);

    var textObj = GameManager.Singleton.ShowText(
      $"-{Mathf.RoundToInt(finalDamage)}",
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

    if (hp <= 0)
    {
      hp = 0;
      Die();
    }
  }
}