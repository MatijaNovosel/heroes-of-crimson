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
  
  private static readonly int AnimationShootingKey = Animator.StringToHash("Shooting");
  private static readonly int AnimationShootKey = Animator.StringToHash("Shoot");
  private static readonly int AnimationIdleStateKey = Animator.StringToHash("IdleState");
  private static readonly int AnimationMouseDirKey = Animator.StringToHash("MouseDir");

  private Vector3 _moveDelta;
  private RaycastHit2D _hit;
  
  // Experience
  public float level = 1;
  public int experience = 0;
  public int xpNeeded = 100;
  
  [Header("Abilities")]
  public AbilitySlot[] abilities = new AbilitySlot[4];
  
  private float _lastFired;
  private bool _isShooting;
  
  public bool HoldingItem;
  public bool InventoryOpen;

  // Talents
  public List<int> learnedTalents = new ();
  
  // Components
  public GameObject Projectile;
  public Animator animator;
  private BoxCollider2D _boxCollider;
  private AnimatorOverrideController _animatorOverrideController;
  private BaseNPCBehaviour _baseNpcBehaviour;
  public Inventory Hotbar;
  public RectTransform TopLeftGroup;
  public PlayerHealthBar HealthBar;
  public PlayerManaBar ManaBar;
  
  public List<Tilemap> forbiddenAbilityTilemaps;
  
  private float _radianceLastTick;
  
  // Stats
  public float actualSwf = 0;
  public float actualMgt = 0;
  public float actualArm = 0;
  public float actualStr = 0;
  public float actualWis = 0;
  public float actualAgi = 0;
  
  private CachedWeaponFireData _cachedWeapon;
  [CanBeNull] private Item _lastWeaponItem;
  private ProjectileSetupModel _cachedProjectileSetup;
  
  private void UpdateWeaponCache()
  {
    var weaponSlot = Hotbar.GetHotbarSlot(0);
    var weaponItem = weaponSlot?.CurrentInventoryItem?.ItemInSlot;

    if (weaponItem?.id == _lastWeaponItem?.id) return;
    _lastWeaponItem = weaponItem;

    _cachedWeapon = new CachedWeaponFireData
    {
      projectileSprite = null,
      projectileDegree = 0,
      projectileScale = 0.6f,
      range = 8f,
      impactColor = Color.white,
      shootSound = ResourceCacher.Singleton.Sounds[Constants.Sounds.MagicShoot],
      minDamage = 0,
      maxDamage = 0
    };

    if (weaponItem)
    {
      _cachedWeapon.projectileSprite = weaponItem.projectileSprite;
      _cachedWeapon.projectileDegree = weaponItem.projectileDegree;
      _cachedWeapon.projectileScale = weaponItem.projectileScale;
      _cachedWeapon.range = weaponItem.range;
      _cachedWeapon.impactColor = weaponItem.impactColor;
      _cachedWeapon.shootSound = ResourceCacher.Singleton.Sounds[weaponItem.shootSound];
      _cachedWeapon.minDamage = weaponItem.minDamage;
      _cachedWeapon.maxDamage = weaponItem.maxDamage;
    }

    _cachedProjectileSetup = new ProjectileSetupModel(
      Vector2.zero,
      _cachedWeapon.projectileDegree,
      null,
      _cachedWeapon.projectileScale,
      0f,
      _cachedWeapon.projectileSprite,
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
      _cachedWeapon.impactColor,
      new(),
      _cachedWeapon.range
    );
  }
  
  private void HandleAbilityCooldowns()
  {
    foreach (var ability in abilities)
    {
      if (ability.cooldownImage is null) continue;
      if (ability.IsReady) ability.cooldownImage.fillAmount = 1f;
      else ability.cooldownImage.fillAmount = ability.CooldownRemaining / ability.cooldown;
    }
  }
  
  private GameObject GetAbilityPrefab(Constants.AbilityType abilityType)
  {
    return abilityType switch
    {
      Constants.AbilityType.Meteor => Resources.Load<GameObject>("Prefabs/Abilities/Meteor"),
      Constants.AbilityType.FireSphere => Resources.Load<GameObject>("Prefabs/Abilities/FireSphere"),
      _ => null
    };
  }

  private bool CanFire()
  {
    return Time.time - _lastFired > GetFireDelay();
  }
  
  public float CalculateWeaponDamage(int minDamage, int maxDamage)
  {
    int rolledBaseDamage = Random.Range(minDamage, maxDamage);

    float multiplier;

    if (_hasStatusEffect(Constants.StatusEffects.Weak)) multiplier = 0.5f;
    else multiplier = 0.5f + (_baseNpcBehaviour.mgt / 50f);

    bool hasDamaging = _hasStatusEffect(Constants.StatusEffects.Damaging);
    if (hasDamaging) multiplier *= 1.25f;

    return rolledBaseDamage * multiplier;
  }

  private void Fire()
  {
    var shootDirection = (Utils.GetMousePosition() - gameObject.transform.position).normalized;
    var angle = Utils.GetAngleFromShootDirection(shootDirection);
    var shootingDirection = Constants.ShootingDirections.UP;

    /*

          ┌─────(90)────┐
          │      │      │
          │      │      │
       (+-180)───╀─────(0)
          │      │      │
          │      │      │
          └────(-90)────┘

    */
    switch (angle)
    {
      case < 135 and > 45:
        // Up
        animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.UP);
        animator.SetFloat(AnimationIdleStateKey, 1);
        break;
      case > -45 and < 45:
        // Right
        shootingDirection =  Constants.ShootingDirections.RIGHT;
        animator.SetFloat(AnimationIdleStateKey, 0);
        transform.localScale = Vector3.one;
        animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      case > 135 and < 180 or > -180 and < -135:
        // Left
        shootingDirection =  Constants.ShootingDirections.LEFT;
        animator.SetFloat(AnimationIdleStateKey, 0);
        transform.localScale = new Vector3(-1, 1, 1);
        animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      default:
        // Down
        shootingDirection =  Constants.ShootingDirections.DOWN;
        animator.SetFloat(AnimationIdleStateKey, 2);
        animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.DOWN);
        break;
    }

    var projectilePosX = gameObject.transform.position.x;
    var projectilePosY = gameObject.transform.position.y;

    switch (shootingDirection)
    {
      case Constants.ShootingDirections.DOWN:
        projectilePosY -= 0.6f;
        break;
      case Constants.ShootingDirections.RIGHT:
        projectilePosX += 0.6f;
        break;
      case Constants.ShootingDirections.LEFT:
        projectilePosX -= 0.6f;
        break;
      case Constants.ShootingDirections.UP:
        projectilePosY += 0.6f;
        break;
    }
    
    var proj = Instantiate(
      Projectile,
      new Vector3(projectilePosX, projectilePosY, 0),
      Quaternion.identity
    );

    AudioManager.Singleton.PlaySound(_cachedWeapon.shootSound);
    
    _cachedProjectileSetup.Direction = shootDirection;
    _cachedProjectileSetup.Damage = CalculateWeaponDamage(
      _cachedWeapon.minDamage,
      _cachedWeapon.maxDamage
    );

    AudioManager.Singleton.PlaySound(_cachedWeapon.shootSound);

    proj.GetComponent<Projectile>().Setup(_cachedProjectileSetup);
    
    _lastFired = Time.time;
  }
  
  private bool IsCursorOverForbiddenAbilityTile()
  {
    Vector3 mouseWorldPos = Utils.GetMousePosition();

    foreach (var tilemap in forbiddenAbilityTilemaps)
    {
      if (!tilemap) continue;
      Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
      if (tilemap.HasTile(cellPos)) return true;
    }

    return false;
  }


  private void HandleMoving()
  {
    var x = Input.GetAxisRaw("Horizontal");
    var y = Input.GetAxisRaw("Vertical");
    _moveDelta = new Vector3(x, y, 0);

    animator.SetFloat("Horizontal", x);
    animator.SetFloat("Vertical", y);
    animator.SetFloat("Speed", _moveDelta.sqrMagnitude);

    if (!_isShooting)
    {
      if (Input.GetKey(KeyCode.W))
      {
        animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Up);
      }

      if (Input.GetKey(KeyCode.S))
      {
        animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Down);
      }

      if (Input.GetKey(KeyCode.A))
      {
        animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Horizonal);
        transform.localScale = new Vector3(-1, 1, 1);
      }

      if (Input.GetKey(KeyCode.D))
      {
        animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Horizonal);
        transform.localScale = Vector3.one;
      }
    }

    var speed = Utils.CalculatePlayerMovementSpeed(_baseNpcBehaviour.swf);
    LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

    var moveY = new Vector2(0, _moveDelta.y);

    if (_hasStatusEffect(Constants.StatusEffects.Paralyzed)) return;
    
    if (!Physics2D.BoxCast(
      transform.position,
      _boxCollider.size, 
      0, 
      moveY, 
      Mathf.Abs(moveY.y * speed), mask)
    )
    {
      transform.Translate(0, moveY.y * speed, 0);
    }

    var moveX = new Vector2(_moveDelta.x, 0);
    
    if (!Physics2D.BoxCast(
      transform.position,
      _boxCollider.size, 
      0, 
      moveX, 
      Mathf.Abs(moveX.x * speed), mask)
    )
    {
      transform.Translate(moveX.x * speed, 0, 0);
    }
  }
  private void HandleShooting()
  {
    var pointerOverUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    var weaponSlot = Hotbar.GetHotbarSlot(0);

    if (
      pointerOverUI || 
      HoldingItem || 
      weaponSlot.CurrentInventoryItem is null || 
      _hasStatusEffect(Constants.StatusEffects.Stunned)
    )
    {
      if (_isShooting)
      {
        _isShooting = false;
        animator.SetBool(AnimationShootingKey, false);
      }
      return;
    }

    if (Input.GetMouseButtonUp(0))
    {
      _isShooting = false;
      animator.SetBool(AnimationShootingKey, false);
      return;
    }

    if (Input.GetMouseButton(0) && CanFire())
    {
      _isShooting = true;
      animator.SetBool(AnimationShootingKey, true);
      animator.SetTrigger(AnimationShootKey);
      Fire();
    }
  }

  private bool _hasStatusEffect(Constants.StatusEffects statusEffect)
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

  public void OnItemEquip(Item item)
  {
    print(item);
  }
  
  public void RestoreHp(int amount)
  {
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

  private void HandleAbility()
  {
    if (_hasStatusEffect(Constants.StatusEffects.Silenced) || ConsoleMenu.Singleton.ConsoleMenuOpen) return;
    
    HandleAbilityCooldowns();

    foreach (var ability in abilities)
    {
      if (!Input.GetKeyDown(ability.key) || !ability.IsReady) continue;

      if (IsCursorOverForbiddenAbilityTile() || _baseNpcBehaviour.mp < ability.manaCost)
      {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
        continue;
      }

      var prefab = GetAbilityPrefab(ability.abilityType);

      _baseNpcBehaviour.mp -= ability.manaCost;
      ability.lastUsedTime = Time.time;

      var cursorPosition = Utils.GetMousePosition();

      switch (ability.abilityType)
      {
        case Constants.AbilityType.Meteor:
        case Constants.AbilityType.FireSphere:
          var abilityObj = Instantiate(
            prefab,
            new Vector3(cursorPosition.x, cursorPosition.y + 0.8f, 0),
            Quaternion.identity
          );

          if (abilityObj.TryGetComponent<Meteor>(out var meteor))
          {
            meteor.Init(cursorPosition);
          }
          else if (abilityObj.TryGetComponent<FireSphere>(out var fireSphere))
          {
            fireSphere.Init();
          }
          
          break;
        case Constants.AbilityType.Teleport:
          transform.position = cursorPosition;
          AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Teleport);
          ParticleManager.Singleton.SpawnParticles(transform, Color.white, 50);
          break;
      }
    }
  }

  private void HandleRegen()
  {
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

  private void HandleManaRegen()
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

  public void LearnTalent(Constants.Talents talent)
  {
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
  }
  
  private float GetAttacksPerSecond()
  {
    float aps = 1.5f + (6.5f * (actualAgi / 75f));
    if (_hasStatusEffect(Constants.StatusEffects.Berserk)) aps *= 1.25f;
    return aps;
  }

  private float GetFireDelay()
  {
    float aps = GetAttacksPerSecond();
    return 1f / aps;
  }

  public void SetStatusEffects(int statusEffectId, float duration)
  {
    _baseNpcBehaviour.ApplyStatusEffect((Constants.StatusEffects)statusEffectId, duration);
  }
  
  private void AddItemStats(InventoryItem item, int[] totals)
  {
    if (item == null || item.ItemInSlot == null) return;
    var stats = item.ItemInSlot.stats;
    for (int i = 0; i < 6; i++) totals[i] += stats[i];
  }

  private void UpdateStats()
  {
    int[] totalStats = new int[6];

    AddItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Weapon)?.CurrentInventoryItem, totalStats);
    AddItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Ability)?.CurrentInventoryItem, totalStats);
    AddItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Armor)?.CurrentInventoryItem, totalStats);
    AddItemStats(Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Accessory)?.CurrentInventoryItem, totalStats);

    actualMgt = _baseNpcBehaviour.mgt + totalStats[(int)Constants.Stats.MGT];
    actualSwf = _baseNpcBehaviour.swf + totalStats[(int)Constants.Stats.SWF];
    actualAgi = _baseNpcBehaviour.agi + totalStats[(int)Constants.Stats.AGI];
    actualArm = _baseNpcBehaviour.arm + totalStats[(int)Constants.Stats.ARM];
    actualStr = _baseNpcBehaviour.str + totalStats[(int)Constants.Stats.STR];
    actualWis = _baseNpcBehaviour.wis + totalStats[(int)Constants.Stats.WIS];
  }

  private void HandleUIKeys()
  {
    if (Input.GetKeyDown(KeyCode.Tab))
    {
      InventoryOpen = !InventoryOpen;
      TopLeftGroup.transform.localScale = InventoryOpen ? Vector3.one : Vector3.zero;
    }
  }
  
  private void LevelUp()
  {
    level++;
    ParticleManager.Singleton.SpawnParticles(
      transform,
      Color.yellow,
      20, 
      "particles_28"
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
      LevelUp();
    }
  }
  
  private void OnWeaponEquipped(Item item)
  {
    Debug.Log($"Equipped weapon: {item.name}");
    if (item.id == 2009)
    {
      _baseNpcBehaviour.ApplyStatusEffect(Constants.StatusEffects.Radiance);
    }
  }

  private void OnWeaponUnequipped(Item item)
  {
    Debug.Log($"Unequipped weapon: {item.name}");
    _lastWeaponItem = null;
    if (item.id == 2009)
    {
      _baseNpcBehaviour.RemoveStatusEffect(Constants.StatusEffects.Radiance);
    }
  }
  
  private void _handleRadiance()
  {
    if (!_hasStatusEffect(Constants.StatusEffects.Radiance)) return;
    if (Time.time - _radianceLastTick < 2f) return;
    _radianceLastTick = Time.time;

    var hits = Physics2D.OverlapCircleAll(
      transform.position,
      4f
    );

    foreach (var col in hits)
    {
      if (!col) continue;
      var collidable = col.GetComponent<Collidable>();
      if (collidable.collisionGroups.All(x => x != Constants.CollisionGroups.Enemy)) continue;
      
      col.SendMessage(
        "ReceiveDamage",
        new DamageModel(
          0f, 
          new List<Constants.StatusEffects>() { Constants.StatusEffects.Burning }
        )
      );
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
    
    _boxCollider = GetComponent<BoxCollider2D>();
    _animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = _animatorOverrideController;
    
    var weaponSlot = Hotbar.GetHotbarSlot((int)Constants.InventorySlotEnum.Weapon);

    weaponSlot.OnItemEquipped += OnWeaponEquipped;
    weaponSlot.OnItemUnequipped += OnWeaponUnequipped;
  }

  private void FixedUpdate()
  {
    HandleMoving();
    UpdateStats();
  }

  private void Awake()
  {
    Singleton = this;
  }

  private void Update()
  {
    UpdateWeaponCache();
    _handleRadiance();
    HandleAbility();
    HandleShooting();
    HandleUIKeys();
    HandleRegen();
    HandleManaRegen();
  }
}
