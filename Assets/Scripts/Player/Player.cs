using System;
using System.Collections;
using System.Collections.Generic;
using GameManagement;
using UnityEngine;
using HeroesOfCrimson.Utils;
using Models;
using TMPro;
using UI.Inventory;
using Unity.Burst.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(BaseNPCBehaviour))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
  private static readonly int AnimationShootingKey = Animator.StringToHash("Shooting");
  private static readonly int AnimationShootKey = Animator.StringToHash("Shoot");
  private static readonly int AnimationIdleStateKey = Animator.StringToHash("IdleState");
  private static readonly int AnimationMouseDirKey = Animator.StringToHash("MouseDir");

  private Vector3 _moveDelta;
  private RaycastHit2D _hit;
  private const float FiringDelay = 0.3f;
  
  [Header("Cursor")]
  public Texture2D normalCursor;
  public Texture2D forbiddenCursor;
  public Vector2 cursorHotspot = Vector2.zero;
  
  // Experience
  public float Level = 1;
  public float Experience = 0;

  [Header("Abilities")]
  public AbilitySlot[] abilities = new AbilitySlot[4];
  
  private float _lastFired;
  private bool _isShooting;
  
  public bool HoldingItem;
  public bool InventoryOpen;

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
  
  // Stats
  public float actualSpd = 0;
  public float actualAtt = 0;
  public float actualDef = 0;
  public float actualVit = 0;
  public float actualWis = 0;
  public float actualDex = 0;
  
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
    // Current game time in seconds - last time fired in game seconds
    return Time.time - _lastFired > FiringDelay;
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

    AudioClip shootSound = null;
    
    Sprite weaponProjectile = null;
    int weaponProjectileDegree = 0;
    var weaponInventorySlot = Hotbar.GetHotbarSlot(0);

    if (weaponInventorySlot.CurrentInventoryItem.ItemInSlot)
    {
      weaponProjectileDegree = weaponInventorySlot.CurrentInventoryItem.ItemInSlot.projectileDegree;
      weaponProjectile = weaponInventorySlot.CurrentInventoryItem.ItemInSlot.projectileSprite;

      switch (weaponInventorySlot.CurrentInventoryItem.ItemInSlot.shootSound)
      {
        case Constants.Sounds.ArrowShoot:
          shootSound = ResourceCacher.Singleton.Sounds[Constants.Sounds.ArrowShoot];
          break;
        default:
          shootSound = ResourceCacher.Singleton.Sounds[Constants.Sounds.MagicShoot];
          break;
      }
    }

    var damage = Utils.RandFloat(
      weaponInventorySlot.CurrentInventoryItem.ItemInSlot.minDamage,
      weaponInventorySlot.CurrentInventoryItem.ItemInSlot.maxDamage
    );
    
    AudioManager.Singleton.PlaySound(shootSound);
    
    proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
      shootDirection,
      weaponProjectileDegree,
      null,
      0.6f,
      (float)damage,
      weaponProjectile,
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
      null
    ));
    
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

    var speed = Utils.CalculatePlayerMovementSpeed(_baseNpcBehaviour.spd);
    LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

    var moveY = new Vector2(0, _moveDelta.y);
    
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

    if (pointerOverUI || HoldingItem || weaponSlot.CurrentInventoryItem is null)
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

  private void HandleAbility()
  {
    HandleAbilityCooldowns();

    for (int i = 0; i < abilities.Length; i++)
    {
      var ability = abilities[i];

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
    // HP per second = 2 + 0.2407 * VIT
    var regenPerSecond = 2f + (0.2407f * actualVit);
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

    actualAtt = _baseNpcBehaviour.att + totalStats[(int)Constants.Stats.ATT];
    actualSpd = _baseNpcBehaviour.spd + totalStats[(int)Constants.Stats.DEF];
    actualDex = _baseNpcBehaviour.dex + totalStats[(int)Constants.Stats.WIS];
    actualDef = _baseNpcBehaviour.def + totalStats[(int)Constants.Stats.VIT];
    actualVit = _baseNpcBehaviour.vit + totalStats[(int)Constants.Stats.DEX];
    actualWis = _baseNpcBehaviour.wis + totalStats[(int)Constants.Stats.SPD];
  }

  private void HandleUIKeys()
  {
    if (Input.GetKeyDown(KeyCode.Tab))
    {
      InventoryOpen = !InventoryOpen;
      TopLeftGroup.transform.localScale = InventoryOpen ? Vector3.one : Vector3.zero;
    }
  }

  private void Start()
  {
    animator = GetComponent<Animator>();
    _boxCollider = GetComponent<BoxCollider2D>();
    _baseNpcBehaviour = GetComponent<BaseNPCBehaviour>();
    _animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = _animatorOverrideController;
  }

  private void FixedUpdate()
  {
    HandleMoving();
    UpdateStats();
  }

  private void Update()
  {
    HandleAbility();
    HandleShooting();
    HandleUIKeys();
    HandleRegen();
    HandleManaRegen();
  }
}
