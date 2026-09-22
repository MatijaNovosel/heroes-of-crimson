using System;
using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class BaseNPCBehaviour : MonoBehaviour
{
  public event Action Died;

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

  public Constants.Sounds deathSound = Constants.Sounds.SkeletonDeath;
  public Constants.Sounds hitSound = Constants.Sounds.GenericHit;
  public GameObject lootBagPrefab;
  
  private GameObject _statusEffectPanel;
  private StatusEffectPanel _statusEffectPanelComponent;

  // Only rebuild the status effect panel when the effect list actually changes
  private bool _statusEffectsDirty;
  private readonly List<Constants.StatusEffects> _effectTypesBuffer = new();

  private void Awake()
  {
    var statusEffectPanelPrefab = Resources.Load<GameObject>("Prefabs/StatusEffectPanel");
    _statusEffectPanel = Instantiate(
      statusEffectPanelPrefab,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      Quaternion.identity
    );
    _statusEffectPanelComponent = _statusEffectPanel.GetComponent<StatusEffectPanel>();
    _statusEffectPanelComponent.Setup(
      ActiveStatusEffects.Select(x => x.Type).ToList(),
      gameObject
    );
    _boxCollider = GetComponent<BoxCollider2D>();
    _lootTable = Constants.LootTables[lootTableId];
  }

  public virtual void Die()
  {
    Died?.Invoke();

    AudioManager.Singleton.PlaySoundCached(deathSound);
    
    if (this.name != "Player")
    {
      if (lootBagPrefab)
      {
        var lootBag = Instantiate(
          lootBagPrefab,
          transform.position,
          Quaternion.identity
        );
        // TODO: Fixaj ovo i stavi da loot table diktira kolko je random itema
        lootBag.GetComponent<LootBag>().GenerateLoot(_lootTable, 2); 
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.LootDrop);
      }
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
    if (!_statusEffectsDirty) return;
    _statusEffectsDirty = false;

    _effectTypesBuffer.Clear();
    for (int i = 0; i < ActiveStatusEffects.Count; i++)
    {
      _effectTypesBuffer.Add(ActiveStatusEffects[i].Type);
    }

    _statusEffectPanelComponent.SetStatusEffects(_effectTypesBuffer);
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
        _statusEffectsDirty = true;
      }
    }
  }

  public void RemoveStatusEffect(Constants.StatusEffects effect)
  {
    var existing = FindStatusEffect(effect);
    if (existing == null) return;
    ActiveStatusEffects.Remove(existing);
    _statusEffectsDirty = true;
  }

  // Plain loops instead of LINQ: these run every FixedUpdate via Move(),
  // and the capturing lambdas allocated garbage on every call.
  private ActiveStatusEffect FindStatusEffect(Constants.StatusEffects effect)
  {
    for (int i = 0; i < ActiveStatusEffects.Count; i++)
    {
      if (ActiveStatusEffects[i].Type == effect) return ActiveStatusEffects[i];
    }
    return null;
  }
  
  public bool HasStatusEffect(Constants.StatusEffects effect)
  {
    return FindStatusEffect(effect) != null;
  }

  private void Update()
  {
    UpdateStatusEffects();
    DisplayStatusEffects();
  }

  public void ApplyStatusEffect(Constants.StatusEffects effect, float duration = 5f)
  {
    var existing = FindStatusEffect(effect);

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
      _statusEffectsDirty = true;
      var data = Utils.GetStatusEffectData(effect);
      
      GameManager.Singleton.ShowText(
        data.Name,
        100,
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
    var minDamage = 1f;
    var maxDamage = 100f;

    var t = Mathf.InverseLerp(minDamage, maxDamage, damage);
    var visualSize = Mathf.Lerp(100f, 145f, t);

    GameManager.Singleton.ShowText(
      Mathf.RoundToInt(damage).ToString(),
      (int)visualSize,
      Color.red,
      new Vector3(transform.position.x, transform.position.y + 0.8f, 0),
      Vector3.up,
      0.5f
    );
  }
  
  private void ReceiveDamage(DamageModel payload)
  {
    AudioManager.Singleton.PlaySoundCached(hitSound);
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