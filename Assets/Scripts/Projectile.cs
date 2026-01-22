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
  private bool _piercing;
  private float _damage = 50;
  private Color _particleColor = Color.white;
  private float _scale = 1;
  private float _rotation = 45;
  private float _moveSpeed = 10f;
  private const float _timeToLive = 2f;
  private const float _frequency = 20.0f;
  private const float _amplitude = 0.5f;
  
  private List<Constants.CollisionGroups> _willDamage = new();
  private List<Constants.CollisionGroups> _willPenetrate = new();

  public void Setup(ProjectileSetupModel payload)
  {
    var spriteRenderer = GetComponent<SpriteRenderer>();
    
    _direction = payload.Direction;

    if (payload.Speed != null)
    {
      _moveSpeed = (float)payload.Speed;
    }

    if (payload.Sprite)
    {
      spriteRenderer.sprite = payload.Sprite;
    }

    if (payload.Rotation != null)
    {
      _rotation = (float)payload.Rotation;
    }

    if (payload.WillDamage.Count != 0)
    {
      _willDamage = payload.WillDamage;
    }
    
    if (payload.WillPenetrate.Count != 0)
    {
      _willPenetrate = payload.WillPenetrate;
    }
    
    if (payload.Scale != null)
    {
      _scale = (float)payload.Scale;
    }
    
    if (payload.Damage != null)
    {
      _damage = (float)payload.Damage;
    }
    
    _particleColor = payload.ParticleColor ?? Color.white;

    _angle = Utils.GetAngleFromShootDirection(payload.Direction);

    gameObject.transform.localScale = new Vector3(_scale, _scale, 0);

    Destroy(gameObject, _timeToLive);
  }

  void Update()
  {
    transform.position += _direction * (_moveSpeed * Time.deltaTime);
    /*

      Projectiles must be rotated according to the sprite
      
        DIR - Normal angle
        DIA - Minus 45 degrees
        RAN - Random

      For sine movement:

        Vector3 pos = transform.position;
        pos += direction * moveSpeed * Time.deltaTime;
        transform.position = pos + transform.right * Mathf.Sin(Time.time * frequency) * amplitude;

    */
    transform.eulerAngles = new Vector3(0, 0, this._angle - this._rotation);
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (!collider) return;
    
    var collidableComponent = collider.gameObject.GetComponent<Collidable>();

    if (!collidableComponent) return;
    
    if (collidableComponent.collisionGroups.Any(x => _willDamage.Contains(x)))
    {
      collider.SendMessage("ReceiveDamage", new DamageModel(_damage));
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
