using System.Collections.Generic;
using GameManagement;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using Models;
using Models.Player;
using UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    private Player _player;
    private Animator _animator;
    private float _lastFired;
    public Inventory hotbar;
    public GameObject projectilePrefab;
    
    private CachedWeaponFireData _cachedWeapon;
    [CanBeNull] private Item _lastWeaponItem;
    private ProjectileSetupModel _cachedProjectileSetup;
    
    private static readonly int AnimationShootingKey = Animator.StringToHash("Shooting");
    private static readonly int AnimationShootKey = Animator.StringToHash("Shoot");
    private static readonly int AnimationMouseDirKey = Animator.StringToHash("MouseDir");
    private static readonly int AnimationIdleStateKey = Animator.StringToHash("IdleState");
    
    private void _updateWeaponCache()
    {
      var weaponSlot = hotbar.GetHotbarSlot(0);
      var weaponItem = weaponSlot?.CurrentInventoryItem?.ItemInSlot;

      if (weaponItem?.id == _lastWeaponItem?.id) return;
      _lastWeaponItem = weaponItem;

      _cachedWeapon = new CachedWeaponFireData
      {
        projectileSprite = null,
        projectileFrames = null,
        projectileDegree = 0,
        projectileScale = 0.6f,
        range = 8f,
        impactColor = Color.white,
        shootSound = ResourceCacher.Singleton.Sounds[Constants.Sounds.MagicShoot],
        minDamage = 0,
        maxDamage = 0,
        spinSpeed = 0
      };

      if (weaponItem)
      {
        _cachedWeapon.projectileSprite = weaponItem.projectileSprite;
        _cachedWeapon.projectileFrames = weaponItem.projectileFrames;
        _cachedWeapon.projectileDegree = weaponItem.projectileDegree;
        _cachedWeapon.projectileScale = weaponItem.projectileScale;
        _cachedWeapon.range = weaponItem.range;
        _cachedWeapon.impactColor = weaponItem.impactColor;
        _cachedWeapon.shootSound = ResourceCacher.Singleton.Sounds[weaponItem.shootSound];
        _cachedWeapon.minDamage = weaponItem.minDamage;
        _cachedWeapon.maxDamage = weaponItem.maxDamage;
        _cachedWeapon.spinSpeed = weaponItem.spinSpeed;
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
        _cachedWeapon.range,
        _cachedWeapon.projectileFrames,
        _cachedWeapon.spinSpeed
      );
    }
    
    private float _getAttacksPerSecond()
    {
      float aps = 1.5f + (6.5f * (_player.actualAgi / 75f));
      if (_player.HasStatusEffect(Constants.StatusEffects.Berserk)) aps *= 1.25f;
      return aps;
    }
    
    private float _getFireDelay()
    {
      float aps = _getAttacksPerSecond();
      return 1f / aps;
    }
    
    private bool _canFire()
    {
      return Time.time - _lastFired > _getFireDelay();
    }
    
    private float _calculateWeaponDamage(int minDamage, int maxDamage)
    {
      int rolledBaseDamage = Random.Range(minDamage, maxDamage);

      float multiplier;

      if (_player.HasStatusEffect(Constants.StatusEffects.Weak)) multiplier = 0.5f;
      else multiplier = 0.5f + (_player.actualMgt / 50f);

      bool hasDamaging = _player.HasStatusEffect(Constants.StatusEffects.Damaging);
      if (hasDamaging) multiplier *= 1.25f;

      return rolledBaseDamage * multiplier;
    }
    
    private void _fire()
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
          _animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.UP);
          _animator.SetFloat(AnimationIdleStateKey, 1);
          break;
        case > -45 and < 45:
          // Right
          shootingDirection =  Constants.ShootingDirections.RIGHT;
          _animator.SetFloat(AnimationIdleStateKey, 0);
          transform.localScale = Vector3.one;
          _animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.HORIZONTAL);
          break;
        case > 135 and < 180 or > -180 and < -135:
          // Left
          shootingDirection =  Constants.ShootingDirections.LEFT;
          _animator.SetFloat(AnimationIdleStateKey, 0);
          transform.localScale = new Vector3(-1, 1, 1);
          _animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.HORIZONTAL);
          break;
        default:
          // Down
          shootingDirection =  Constants.ShootingDirections.DOWN;
          _animator.SetFloat(AnimationIdleStateKey, 2);
          _animator.SetFloat(AnimationMouseDirKey, (int)Constants.ShootingMouseDirs.DOWN);
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
        projectilePrefab,
        new Vector3(projectilePosX, projectilePosY, 0),
        Quaternion.identity
      );

      AudioManager.Singleton.PlaySound(_cachedWeapon.shootSound);
      
      _cachedProjectileSetup.Direction = shootDirection;
      _cachedProjectileSetup.Damage = _calculateWeaponDamage(
        _cachedWeapon.minDamage,
        _cachedWeapon.maxDamage
      );

      AudioManager.Singleton.PlaySound(_cachedWeapon.shootSound);

      proj.GetComponent<Projectile>().Setup(_cachedProjectileSetup);
      
      _lastFired = Time.time;
    }
    
    private void _handleShooting()
    {
      var pointerOverUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
      var weaponSlot = hotbar.GetHotbarSlot(0);

      if (
        pointerOverUI || 
        _player.HoldingItem || 
        weaponSlot.CurrentInventoryItem is null || 
        _player.HasStatusEffect(Constants.StatusEffects.Stunned)
      )
      {
        if (_player.isShooting)
        {
          _player.isShooting = false;
          _animator.SetBool(AnimationShootingKey, false);
        }
        return;
      }

      if (Input.GetMouseButtonUp(0))
      {
        _player.isShooting = false;
        _animator.SetBool(AnimationShootingKey, false);
        return;
      }

      if (Input.GetMouseButton(0) && _canFire())
      {
        _player.isShooting = true;
        _animator.SetBool(AnimationShootingKey, true);
        _animator.SetTrigger(AnimationShootKey);
        _fire();
      }
    }

    public void SetLastWeapon(Item item)
    {
      _lastWeaponItem = item;
    }
    
    void Start()
    {
        _player = GetComponent<Player>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
      _updateWeaponCache();
      _handleShooting();
    }
}
