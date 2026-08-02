using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
  [SerializeField] private GameObject impactParticlePrefab;
  [SerializeField] private float frameDuration = 0.1f;

  private SpriteRenderer _spriteRenderer;
  private Vector3 _direction;
  private float _angle;
  private float _damage = 50;
  private Color _particleColor = Color.white;
  private float _scale = 1;
  private float _rotation = 45;
  private float _moveSpeed = 10f;
  private float _range = 10f;
  private Vector3 _startPosition;
  private float _spinSpeed;
  private float _spinAngle;

  private List<Constants.CollisionGroups> _willDamage = new();
  private List<Constants.CollisionGroups> _willPenetrate = new();
  private List<Constants.StatusEffects> _statusEffects = new();

  private List<Sprite> _frames = new();
  private int _frameIndex;
  private float _frameTimer;

  private void Awake()
  {
    _spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void Setup(ProjectileSetupModel payload)
  {
    _direction = payload.Direction.normalized;
    _startPosition = transform.position;

    if (payload.Speed != null) _moveSpeed = (float)payload.Speed;
    if (payload.Range != null) _range = (float)payload.Range;
    if (payload.Rotation != null) _rotation = (float)payload.Rotation;
    if (payload.WillDamage.Count != 0) _willDamage = payload.WillDamage;
    if (payload.WillPenetrate.Count != 0) _willPenetrate = payload.WillPenetrate;
    if (payload.Scale != null) _scale = (float)payload.Scale;
    if (payload.Damage != null) _damage = (float)payload.Damage;
    if (payload.StatusEffects.Count != 0) _statusEffects = payload.StatusEffects;

    _particleColor = payload.ParticleColor ?? Color.white;
    _angle = Utils.GetAngleFromShootDirection(payload.Direction);

    _spinAngle = 0f;
    _spinSpeed = payload.SpinSpeed ?? 0f;

    if (payload.Frames != null && payload.Frames.Count > 0)
    {
      _frames = payload.Frames;
      _frameIndex = 0;
      _frameTimer = 0f;
      _spriteRenderer.sprite = _frames[0];
    }
    else
    {
      _spriteRenderer.sprite = payload.Sprite;
    }

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

    _spinAngle += _spinSpeed * Time.deltaTime;

    transform.rotation = Quaternion.Euler(
      0,
      0,
      _angle - _rotation + _spinAngle
    );

    if (_frames.Count > 0)
    {
      _frameTimer += Time.deltaTime;
      if (_frameTimer >= frameDuration)
      {
        _frameTimer = 0f;
        _frameIndex = (_frameIndex + 1) % _frames.Count;
        _spriteRenderer.sprite = _frames[_frameIndex];
      }
    }
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
        new DamageModel(_damage, _statusEffects)
      );
    }

    if (collidableComponent.collisionGroups.Any(x => _willPenetrate.Contains(x)))
    {
      return;
    }

    if (collider.name != "BulletCollision")
    {
      ParticleManager.Singleton.SpawnParticles(transform, _particleColor, 6);
      if (collider.name == "Collision")
      {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.HitWall);
      }
    }

    Destroy(gameObject);
  }
}