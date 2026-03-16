using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using Models;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
  [SerializeField] private GameObject impactParticlePrefab;
  
  private Vector3 _direction;
  private float _angle;
  private float _damage = 50;
  private Color _particleColor = Color.white;
  private float _scale = 1;
  private float _rotation = 45;
  private float _moveSpeed = 10f;
  private const float _timeToLive = 2f;
  private float _range = 10f;
  private Vector3 _startPosition;
  
  private List<Constants.CollisionGroups> _willDamage = new();
  private List<Constants.CollisionGroups> _willPenetrate = new();
  private List<Constants.StatusEffects> _statusEffects = new();

  public void Setup(ProjectileSetupModel payload)
  {
    var spriteRenderer = GetComponent<SpriteRenderer>();

    _direction = payload.Direction.normalized;
    _startPosition = transform.position;

    if (payload.Speed != null) _moveSpeed = (float)payload.Speed;
    if (payload.Range != null) _range = (float)payload.Range;
    if (payload.Sprite) spriteRenderer.sprite = payload.Sprite;
    if (payload.Rotation != null) _rotation = (float)payload.Rotation;
    if (payload.WillDamage.Count != 0) _willDamage = payload.WillDamage;
    if (payload.WillPenetrate.Count != 0) _willPenetrate = payload.WillPenetrate;
    if (payload.Scale != null) _scale = (float)payload.Scale;
    if (payload.Damage != null) _damage = (float)payload.Damage;
    if (payload.StatusEffects.Count != 0) _statusEffects = payload.StatusEffects;

    _particleColor = payload.ParticleColor ?? Color.white;
    _angle = Utils.GetAngleFromShootDirection(payload.Direction);

    transform.localScale = new Vector3(_scale, _scale, 0);
  }

  void Update()
  {
    transform.position += _direction * (_moveSpeed * Time.deltaTime);
    float traveled = Vector3.Distance(_startPosition, transform.position);
    
    if (traveled >= _range)
    {
      Destroy(gameObject);
      return;
    }

    transform.eulerAngles = new Vector3(0, 0, _angle - _rotation);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (!collider) return;
    
    var collidableComponent = collider.gameObject.GetComponent<Collidable>();

    if (!collidableComponent) return;
    
    if (collidableComponent.collisionGroups.Any(x => _willDamage.Contains(x)))
    {
      collider.SendMessage(
        Constants.NPCMessages.ReceiveDamage, 
        new DamageModel(_damage, this._statusEffects)
      );
    }
    
    if (collidableComponent.collisionGroups.Any(x => _willPenetrate.Contains(x)))
    {
      return;
    }

    if (collider.name != "BulletCollision")
    {
      ParticleManager.Singleton.SpawnParticles(transform, _particleColor, 6);
    }
    
    Destroy(gameObject);
  }
}
