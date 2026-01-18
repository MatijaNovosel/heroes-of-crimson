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
using UnityEngine.UI;

[RequireComponent(typeof(BaseNPCBehaviour))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
  private static readonly int Shooting = Animator.StringToHash("Shooting");
  private static readonly int Shoot = Animator.StringToHash("Shoot");
  private static readonly int IdleState = Animator.StringToHash("IdleState");
  private static readonly int MouseDir = Animator.StringToHash("MouseDir");

  private Vector3 _moveDelta;
  private RaycastHit2D _hit;
  private const float FiringDelay = 0.3f;
  
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
  public TMP_Text HealthbarRegenText;
  public TMP_Text ManaBarRegenText;
  
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

      if (ability.IsReady)
      {
        ability.cooldownImage.fillAmount = 1f;
      }
      else
      {
        ability.cooldownImage.fillAmount = ability.CooldownRemaining / ability.cooldown;
      }
    }
  }
  
  private GameObject GetAbilityPrefab(int index)
  {
    return index switch
    {
      0 => Resources.Load<GameObject>("Prefabs/Abilities/Meteor"),
      1 => Resources.Load<GameObject>("Prefabs/Abilities/FireSphere"),
      2 => Resources.Load<GameObject>("Prefabs/Abilities/FireSphere"),
      3 => Resources.Load<GameObject>("Prefabs/Abilities/FireSphere"),
      _ => null
    };
  }

  private bool CanFire()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - _lastFired > FiringDelay;
  }
  
  private float GetManaRegenPerSecond()
  {
    // MP per second = 0.5 + 0.12 * WIS
    return Mathf.Max(0f, 0.5f + (0.12f * actualWis));
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
        animator.SetFloat(MouseDir, (int)Constants.ShootingMouseDirs.UP);
        animator.SetFloat(IdleState, 1);
        break;
      case > -45 and < 45:
        // Right
        shootingDirection =  Constants.ShootingDirections.RIGHT;
        animator.SetFloat(IdleState, 0);
        transform.localScale = Vector3.one;
        animator.SetFloat(MouseDir, (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      case > 135 and < 180 or > -180 and < -135:
        // Left
        shootingDirection =  Constants.ShootingDirections.LEFT;
        animator.SetFloat(IdleState, 0);
        transform.localScale = new Vector3(-1, 1, 1);
        animator.SetFloat(MouseDir, (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      default:
        // Down
        shootingDirection =  Constants.ShootingDirections.DOWN;
        animator.SetFloat(IdleState, 2);
        animator.SetFloat(MouseDir, (int)Constants.ShootingMouseDirs.DOWN);
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
    
    AudioSource.PlayClipAtPoint(shootSound, transform.position, 1.5f);
    
    proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
      shootDirection,
      weaponProjectileDegree,
      null,
      0.6f,
      50,
      weaponProjectile,
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
      null
    ));
    
    _lastFired = Time.time;
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
        animator.SetFloat(IdleState, (int)Constants.AnimationIdleState.Up);
      }

      if (Input.GetKey(KeyCode.S))
      {
        animator.SetFloat(IdleState, (int)Constants.AnimationIdleState.Down);
      }

      if (Input.GetKey(KeyCode.A))
      {
        animator.SetFloat(IdleState, (int)Constants.AnimationIdleState.Horizonal);
        transform.localScale = new Vector3(-1, 1, 1);
      }

      if (Input.GetKey(KeyCode.D))
      {
        animator.SetFloat(IdleState, (int)Constants.AnimationIdleState.Horizonal);
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
        animator.SetBool(Shooting, false);
      }
      return;
    }

    if (Input.GetMouseButtonUp(0))
    {
      _isShooting = false;
      animator.SetBool(Shooting, false);
      return;
    }

    if (Input.GetMouseButton(0) && CanFire())
    {
      _isShooting = true;
      animator.SetBool(Shooting, true);
      animator.SetTrigger(Shoot);
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

      if (_baseNpcBehaviour.mp < ability.manaCost)
      {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
        return;
      }

      var prefab = GetAbilityPrefab(i);
      if (!prefab) return;

      _baseNpcBehaviour.mp -= ability.manaCost;
      ability.lastUsedTime = Time.time;

      var cursorPosition = Utils.GetMousePosition();

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
    }
  }

  private void HandleRegen()
  {
    // HP per second = 2 + 0.2407 * VIT
    var regenPerSecond = 2f + (0.2407f * actualVit);
    regenPerSecond = Mathf.Max(0f, regenPerSecond);
    
    HealthbarRegenText.text = $"+{regenPerSecond:F2}";
    _baseNpcBehaviour.hp += regenPerSecond * Time.deltaTime;
    _baseNpcBehaviour.hp = Mathf.Min(
      _baseNpcBehaviour.hp,
      _baseNpcBehaviour.maxHp
    );
  }

  private void HandleManaRegen()
  {
    var manaRegenPerSecond = GetManaRegenPerSecond();
    
    ManaBarRegenText.text = $"+{manaRegenPerSecond:F2}";
    _baseNpcBehaviour.mp += manaRegenPerSecond * Time.deltaTime;
    _baseNpcBehaviour.mp = Mathf.Min(
      _baseNpcBehaviour.mp,
      _baseNpcBehaviour.maxMp
    );
  }

  private void UpdateStats()
  {
    var weaponSlot = Hotbar.GetHotbarSlot(0);
    var abilitySlot = Hotbar.GetHotbarSlot(1);
    var armorSlot = Hotbar.GetHotbarSlot(2);
    var accessorySlot = Hotbar.GetHotbarSlot(3);

    // ATT, SPD, DEX, DEF, VIT, WIS
    var totalStats = new List<int> { 0, 0, 0, 0, 0, 0 };
    
    var weaponItem = weaponSlot.CurrentInventoryItem;
    var abilityItem = abilitySlot.CurrentInventoryItem;
    var armorItem = armorSlot.CurrentInventoryItem;
    var accessoryItem = accessorySlot.CurrentInventoryItem;
    
    if (weaponItem)
    {
      for (var i = 0; i < 6; i++) totalStats[i] += weaponItem.ItemInSlot.stats[i];
    }
    
    if (abilityItem)
    {
      for (var i = 0; i < 6; i++) totalStats[i] += abilityItem.ItemInSlot.stats[i];
    }
    
    if (armorItem)
    {
      for (var i = 0; i < 6; i++) totalStats[i] += armorItem.ItemInSlot.stats[i];
    }
    
    if (accessoryItem)
    {
      for (var i = 0; i < 6; i++) totalStats[i] += accessoryItem.ItemInSlot.stats[i];
    }

    actualAtt = _baseNpcBehaviour.att + totalStats[0];
    actualSpd = _baseNpcBehaviour.spd + totalStats[1];
    actualDex = _baseNpcBehaviour.def + totalStats[2];
    actualDef = _baseNpcBehaviour.def + totalStats[3];
    actualVit = _baseNpcBehaviour.vit + totalStats[4];
    actualWis = _baseNpcBehaviour.wis + totalStats[5];
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
