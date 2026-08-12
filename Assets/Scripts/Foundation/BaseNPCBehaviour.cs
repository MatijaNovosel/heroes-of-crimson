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
  
  public Constants.LootTableEnum lootTableId = Constants.LootTableEnum.Basic;
  private LootTableModel _lootTable;

  public AudioClip deathSound;
  public AudioClip hitSound;
  public GameObject lootBagPrefab;
  
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
    _lootTable = Constants.LootTables[lootTableId];
  }

  public void Die()
  {
    if (deathSound) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.5f);
    
    if (this.name != "Player")
    {
        var lootBag = Instantiate(
          lootBagPrefab,
          transform.position,
          Quaternion.identity
        );
        
        lootBag.GetComponent<LootBag>().GenerateLoot(_lootTable, 3);
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.LootDrop);
        Player.Singleton.GiveXp(xpValue);
    }
    else
    {
      Player.Singleton.OnDeath();
    }

    Destroy(gameObject);
  }
  
  public void Move(Vector2 direction)
  {
    if (_boxCollider == null) return;
    if (direction.sqrMagnitude <= 0.0001f) return;

    direction = direction.normalized;

    if (HasStatusEffect(Constants.StatusEffects.Paralyzed)) return;

    bool slowed = HasStatusEffect(Constants.StatusEffects.Slowed);
    bool energized = HasStatusEffect(Constants.StatusEffects.Energized);

    float step = Utils.CalculateMoveStepDistance(swf, slowed, energized, Time.fixedDeltaTime);

    LayerMask mask;

    if (gameObject.layer == LayerMask.NameToLayer("Critter"))
    {
      mask = LayerMask.GetMask("Blocking", "NPC", "Critter");
    }
    else
    {
      mask = LayerMask.GetMask("Actor", "Blocking", "NPC"); 
    }

    var moveY = new Vector2(0f, direction.y);
    if (moveY.y != 0f &&
        !Physics2D.BoxCast(
          transform.position,
          _boxCollider.size,
          0,
          moveY,
          step,
          mask
    ))
    {
      transform.Translate(0f, moveY.y * step, 0f);
    }

    var moveX = new Vector2(direction.x, 0f);
    if (moveX.x != 0f &&
        !Physics2D.BoxCast(
          transform.position,
          _boxCollider.size,
          0,
          moveX,
          step,
          mask
    ))
    {
      transform.Translate(moveX.x * step, 0f, 0f);
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
        case Constants.StatusEffects.Burning:
          if (now >= effect.NextTickTime)
          {
            ReceiveDamage(new DamageModel(20f, new List<Constants.StatusEffects>()));
            effect.NextTickTime = now + 2f;
          }
          break;
        case Constants.StatusEffects.Bleeding:
          if (now >= effect.NextTickTime)
          {
            ReceiveDamage(new DamageModel(10f, new List<Constants.StatusEffects>()));
            effect.NextTickTime = now + 1f;
          }
          break;
        case Constants.StatusEffects.Poisoned:
          if (now >= effect.NextTickTime)
          {
            ReceiveDamage(new DamageModel(10f, new List<Constants.StatusEffects>()));
            effect.NextTickTime = now + 1f;
          }
          break;
        case Constants.StatusEffects.ArmorBroken:
          arm = 0;
          break;
      }

      if (!effect.Permanent && effect.ExpireTime <= now)
      {
        ActiveStatusEffects.RemoveAt(i);
      }
    }
  }

  public void RemoveStatusEffect(Constants.StatusEffects effect)
  {
    var existing = ActiveStatusEffects.FirstOrDefault(e => e.Type == effect);
    if (existing != null) ActiveStatusEffects.Remove(existing);
  }
  
  public bool HasStatusEffect(Constants.StatusEffects effect)
  {
    var existing = ActiveStatusEffects.Any(e => e.Type == effect);
    return existing;
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
      var shouldBePermanent = effect == Constants.StatusEffects.Radiance;
      ActiveStatusEffects.Add(new ActiveStatusEffect(
        effect,
        duration,
        shouldBePermanent
      ));
      var data = Utils.GetStatusEffectData(effect);
      
      GameManager.Singleton.ShowText(
        data.Name,
        500,
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

  private void _showDamageText(float damage)
  {
    var angle = Random.Range(-40f, 40f);
    var radians = angle * Mathf.Deg2Rad;
    var randomDirection = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f);
    var randomRotation = Random.Range(-15f, 15f);

    var minDamage = 1f;
    var maxDamage = 100f;
    var minFontSize = 400f;
    var maxFontSize = 580f;

    var t = Mathf.InverseLerp(minDamage, maxDamage, damage);
    var fontSize = Mathf.Lerp(minFontSize, maxFontSize, t);
    var randomScale = Random.Range(0.9f, 1.2f);

    var textObj = GameManager.Singleton.ShowText(
      Mathf.RoundToInt(damage).ToString(),
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
  }
  
  private void ReceiveDamage(DamageModel payload)
  {
    if (hitSound) AudioManager.Singleton.PlaySound(hitSound);
    foreach (var effect in payload.StatusEffects) ApplyStatusEffect(effect);
    if (invincible) return;

    float finalDamage = CalculateDamageAfterDefense(payload.Value);
    hp -= finalDamage;
    if (finalDamage > 0) _showDamageText(finalDamage);

    if (hp <= 0)
    {
      hp = 0;
      Die();
    }
  }
}