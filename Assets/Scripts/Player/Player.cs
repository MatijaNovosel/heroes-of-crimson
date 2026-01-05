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

  private float _lastFired;
  private float _abilityUsedLast;
  private const float FiringDelay = 0.3f;
  private const float AbilityDelay = 1;
  private float _abilityCooldownTimer;
  private bool _isShooting;
  
  public bool HoldingItem;
  public bool InventoryOpen;
  public bool PauseMenuOpen;

  // Components
  public GameObject Projectile;
  public Image abilityImage;
  public Animator animator;
  private BoxCollider2D _boxCollider;
  private AnimatorOverrideController _animatorOverrideController;
  private BaseNPCBehaviour _baseNpcBehaviour;
  public Inventory Hotbar;
  public RectTransform TopLeftGroup;
  public TMP_Text HealthbarRegenText;
  
  // Stats
  public float actualSpd = 0;
  public float actualAtt = 0;
  public float actualDef = 0;
  public float actualVit = 0;
  public float actualWis = 0;
  public float actualDex = 0;

  private bool CanFire()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - _lastFired > FiringDelay;
  }

  private bool CanCastAbility()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - _abilityUsedLast > AbilityDelay;
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
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player }
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

  private void HandleAbilityCooldown()
  {
    _abilityCooldownTimer -= Time.deltaTime;

    if (_abilityCooldownTimer < 0.0f)
    {
      abilityImage.fillAmount = 1.0f;
    }
    else
    {
      abilityImage.fillAmount = _abilityCooldownTimer / AbilityDelay;
    }
  }

  private void HandleAbility()
  {
    HandleAbilityCooldown();
    
    if (!Input.GetKey(KeyCode.R) || !CanCastAbility()) return;
    
    var cursorPosition = Utils.GetMousePosition();
    var meteorPrefab = Resources.Load<GameObject>("Prefabs/Meteor");

    if (meteorPrefab is null) return;

    _abilityCooldownTimer = AbilityDelay;

    var meteor = Instantiate(
      meteorPrefab,
      new Vector3(cursorPosition.x, cursorPosition.y + 0.8f, 0),
      Quaternion.identity
    );

    meteor.GetComponent<Meteor>().Setup(cursorPosition);

    _abilityUsedLast = Time.time;
  }

  private void HandleRegen()
  {
    var regenPerSecond = Mathf.Max(0f, actualVit * 0.1f);
    HealthbarRegenText.text = $"+{regenPerSecond}";
    _baseNpcBehaviour.hp += regenPerSecond * Time.deltaTime;
    _baseNpcBehaviour.hp = Mathf.Min(
      _baseNpcBehaviour.hp,
      _baseNpcBehaviour.maxHp
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
  }
}
