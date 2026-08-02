using System;
using System.Collections.Generic;
using System.Linq;
using GameManagement;
using UnityEngine;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using Models;
using Models.Player;
using UI;
using UI.Inventory;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BaseNPCBehaviour))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
  public static Player Singleton;
  
  // Experience
  public int level = 1;
  public int experience = 0;
  public int xpNeeded = 100;
  
  // Flags
  [HideInInspector]
  public bool isShooting;
  [HideInInspector]
  public bool HoldingItem;
  [HideInInspector]
  public bool InventoryOpen;

  // Talents
  public List<int> learnedTalents = new ();
  
  // Components
  [HideInInspector]
  public Animator animator;
  private AnimatorOverrideController _animatorOverrideController;
  private BaseNPCBehaviour _baseNpcBehaviour;
  public Inventory Hotbar;
  public RectTransform TopLeftGroup;
  public PlayerHealthBar HealthBar;
  public PlayerManaBar ManaBar;
  
  private PlayerShooting _playerShootingComponent;
  
  private float _radianceLastTick;
  
  // Stats
  public float actualSwf = 0;
  public float actualMgt = 0;
  public float actualArm = 0;
  public float actualStr = 0;
  public float actualWis = 0;
  public float actualAgi = 0;
  
  public float CurrentMana
  {
    get => _baseNpcBehaviour.mp;
    set => _baseNpcBehaviour.mp = value;
  }
  
  public float CurrentHealth
  {
    get => _baseNpcBehaviour.hp;
    set => _baseNpcBehaviour.hp = value;
  }
  
  public float MaxHealth => _baseNpcBehaviour.maxHp;
  public float MaxMana => _baseNpcBehaviour.maxMp;
  
  public bool IsDead => _baseNpcBehaviour.hp <= 0;

  public bool HasStatusEffect(Constants.StatusEffects statusEffect)
  {
    return _baseNpcBehaviour.ActiveStatusEffects.Any(x => x.Type == statusEffect);
  }

  public void IncreaseMaxHp(int amount)
  {
    _baseNpcBehaviour.maxHp += amount;
    _baseNpcBehaviour.hp += amount;
    _baseNpcBehaviour.hp = Mathf.Min(
      _baseNpcBehaviour.hp,
      _baseNpcBehaviour.maxHp
    );
  }
  
  public void RestoreHp(int amount)
  {
    if (HasStatusEffect(Constants.StatusEffects.Sick)) return;
    _baseNpcBehaviour.hp += amount;
    _baseNpcBehaviour.hp = Mathf.Min(
      _baseNpcBehaviour.hp,
      _baseNpcBehaviour.maxHp
    );
  }
  
  public void RestoreMp(int amount)
  {
    _baseNpcBehaviour.mp += amount;
    _baseNpcBehaviour.mp = Mathf.Min(
      _baseNpcBehaviour.mp,
      _baseNpcBehaviour.maxMp
    );
  }

  private void _handleRegen()
  {
    if (HasStatusEffect(Constants.StatusEffects.Sick)) return;
    
    // HP per second = 2 + 0.2407 * STR
    var regenPerSecond = 2f + (0.2407f * actualStr);
    regenPerSecond = Mathf.Max(0f, regenPerSecond);
    
    HealthBar.UpdateRegenText(regenPerSecond);
    _baseNpcBehaviour.hp += regenPerSecond * Time.deltaTime;
    _baseNpcBehaviour.hp = Mathf.Min(
      _baseNpcBehaviour.hp,
      _baseNpcBehaviour.maxHp
    );
  }

  private void _handleManaRegen()
  {
    var regenPerSecond = 0.5f + (0.12f * actualWis);
    var manaRegenPerSecond = Mathf.Max(0f, regenPerSecond);
    
    ManaBar.UpdateRegenText(manaRegenPerSecond);
    _baseNpcBehaviour.mp += manaRegenPerSecond * Time.deltaTime;
    _baseNpcBehaviour.mp = Mathf.Min(
      _baseNpcBehaviour.mp,
      _baseNpcBehaviour.maxMp
    );
  }

  public void LearnTalent(int talentId)
  {
    /*
    if (learnedTalents.Contains((int)talent)) return;
    learnedTalents.Add((int)talent);
    if (talent == Constants.Talents.ArcaneSupremacyOne)
    {
      _baseNpcBehaviour.maxMp += 20;
      _baseNpcBehaviour.mp += 20;
      _baseNpcBehaviour.mp = Mathf.Min(
        _baseNpcBehaviour.mp,
        _baseNpcBehaviour.maxMp
      );
    }
    */
  }

  public void SetStatusEffect(int statusEffectId, float duration)
  {
    _baseNpcBehaviour.ApplyStatusEffect((Constants.StatusEffects)statusEffectId, duration);
  }
  
  private void _addItemStats(InventoryItem item, int[] totals)
  {
    if (item == null || item.ItemInSlot == null) return;
    var stats = item.ItemInSlot.stats;
    for (int i = 0; i < 6; i++) totals[i] += stats[i];
  }

  private void _updateStats()
  {
    int[] totalStats = new int[6];

    _addItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Weapon)?.CurrentInventoryItem, totalStats);
    _addItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Armor)?.CurrentInventoryItem, totalStats);
    _addItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Accessory1)?.CurrentInventoryItem, totalStats);
    _addItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Accessory2)?.CurrentInventoryItem, totalStats);

    actualMgt = _baseNpcBehaviour.mgt + totalStats[(int)Constants.Stats.MGT];
    actualSwf = _baseNpcBehaviour.swf + totalStats[(int)Constants.Stats.SWF];
    actualAgi = _baseNpcBehaviour.agi + totalStats[(int)Constants.Stats.AGI];
    actualArm = _baseNpcBehaviour.arm + totalStats[(int)Constants.Stats.ARM];
    actualStr = _baseNpcBehaviour.str + totalStats[(int)Constants.Stats.STR];
    actualWis = _baseNpcBehaviour.wis + totalStats[(int)Constants.Stats.WIS];
  }

  private void _handleUIKeys()
  {
    if (Input.GetKeyDown(KeyCode.Tab) && !TalentTree.Singleton.TalentTreeOpen)
    {
      InventoryOpen = !InventoryOpen;
      TopLeftGroup.transform.localScale = InventoryOpen ? Vector3.one : Vector3.zero;
    }
  }
  
  private void _levelUp()
  {
    level++;
    ParticleManager.Singleton.SpawnParticles(
      transform,
      Color.yellow,
      20, 
      "projectiles_25"
    );
    PlayerLog.Singleton.AddItem($"<color=#F1C40F>Level up!</color> You are now level {level}!");
  }

  public void GiveXp(int amount)
  {
    experience += amount;
    PlayerLog.Singleton.AddItem($"You got {amount} xp!");

    while (experience >= xpNeeded)
    {
      experience -= xpNeeded;
      _levelUp();
    }
  }
  
  private void _onWeaponEquipped(Item item)
  {
    if (item.id == (int)WeaponItemEnum.Radiance)
    {
      _baseNpcBehaviour.ApplyStatusEffect(Constants.StatusEffects.Radiance);
      PlayerLog.Singleton.AddItem("<color=#F1C40F>You feel a great warmth around you.</color>");
    }
  }

  public void OnDeath()
  {
    DeathOverlay.Singleton.Show();
  }

  private void _onWeaponUnequipped(Item item)
  {
    _playerShootingComponent.SetLastWeapon(null);
    if (item.id == (int)WeaponItemEnum.Radiance)
    {
      _baseNpcBehaviour.RemoveStatusEffect(Constants.StatusEffects.Radiance);
    }
  }

  public void TeleportToMarker(Constants.TeleportMarkers markerId)
  {
    var marker = GameManager.Singleton.teleportMarkersDict[markerId];
    this.transform.position = marker.transform.position;
  }

  public void Kill()
  {
    _baseNpcBehaviour.Die();
  }
  
  private void _handleItemEffects()
  {
    if (HasStatusEffect(Constants.StatusEffects.Radiance) && Time.time - _radianceLastTick > 2f)
    {
      _radianceLastTick = Time.time;

      var hits = Physics2D.OverlapCircleAll(
        transform.position,
        4f
      );

      foreach (var col in hits)
      {
        if (!col) continue;
        var collidable = col.GetComponent<Collidable>();
        if (!collidable) continue;
        if (collidable.collisionGroups.All(x => x != Constants.CollisionGroups.Enemy)) continue;
      
        col.SendMessage(
          Constants.NPCMessages.ReceiveDamage,
          new DamageModel(
            0f, 
            new List<Constants.StatusEffects>() { Constants.StatusEffects.Burning }
          )
        );
      }
    }
  }

  private void Start()
  {
    animator = GetComponent<Animator>();
    _baseNpcBehaviour = GetComponent<BaseNPCBehaviour>();
    var playerSpritePath = "Animations/Player/Mage/PlayerMage";
    
    switch (GameManager.Singleton.GetSelectedCharacter())
    {
      case (int)Constants.Character.Mage:
        playerSpritePath = "Animations/Player/Mage/PlayerMage";
        _baseNpcBehaviour.mp = 200f;
        _baseNpcBehaviour.maxMp = 200f;
        _baseNpcBehaviour.hp = 75f;
        _baseNpcBehaviour.maxHp = 75f;
        break;
      case (int)Constants.Character.Knight:
        playerSpritePath = "Animations/Player/Knight/PlayerKnight";
        _baseNpcBehaviour.mp = 100f;
        _baseNpcBehaviour.maxMp = 100f;
        _baseNpcBehaviour.hp = 125f;
        _baseNpcBehaviour.maxHp = 125f;
        break;
      case (int)Constants.Character.Ranger:
        playerSpritePath = "Animations/Player/Ranger/PlayerRanger";
        _baseNpcBehaviour.mp = 100f;
        _baseNpcBehaviour.maxMp = 100f;
        _baseNpcBehaviour.hp = 100f;
        _baseNpcBehaviour.maxHp = 100f;
        break;
    }
    
    animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(playerSpritePath);
    _animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = _animatorOverrideController;
    
    var weaponSlot = Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Weapon);

    weaponSlot.OnItemEquipped += _onWeaponEquipped;
    weaponSlot.OnItemUnequipped += _onWeaponUnequipped;
  }

  private void FixedUpdate()
  {
    _updateStats();
  }

  private void Awake()
  {
    Singleton = this;
    _playerShootingComponent = GetComponent<PlayerShooting>();
  }

  private void Update()
  {
    _handleItemEffects();
    _handleUIKeys();
    _handleRegen();
    _handleManaRegen();
  }
}
