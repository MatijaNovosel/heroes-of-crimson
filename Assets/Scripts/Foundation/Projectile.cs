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
  private float _distance;
  private Vector3 _perpendicular;
  private float _waveAmplitude;
  private float _waveNumber;
  private float _wavePhase;

  private List<Constants.CollisionGroups> _willDamage = new();
  private List<Constants.CollisionGroups> _willPenetrate = new();
  private List<Constants.StatusEffects> _statusEffects = new();

  [System.NonSerialized] public bool SilentWallHits;

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

    _distance = 0f;
    _perpendicular = new Vector3(-_direction.y, _direction.x, 0f);
    bool weaves = payload.WaveAmplitude > 0f && payload.WaveLength > 0f;
    _waveAmplitude = weaves ? payload.WaveAmplitude : 0f;
    _waveNumber = weaves ? 2f * Mathf.PI / payload.WaveLength : 0f;
    _wavePhase = payload.WavePhase;

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
    UpdateMovement(0f);
  }

  void Update()
  {
    _distance += _moveSpeed * Time.deltaTime;

    if (_distance >= _range)
    {
      Destroy(gameObject);
      return;
    }

    UpdateMovement(Time.deltaTime);

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

  private void UpdateMovement(float deltaTime)
  {
    float sideOffset = 0f;
    float headingOffset = 0f;

    if (_waveAmplitude > 0f)
    {
      float theta = _waveNumber * _distance + _wavePhase;
      sideOffset = _waveAmplitude * Mathf.Sin(theta);
      float slope = _waveAmplitude * _waveNumber * Mathf.Cos(theta);
      headingOffset = Mathf.Atan(slope) * Mathf.Rad2Deg;
    }

    transform.position = _startPosition + _direction * _distance + _perpendicular * sideOffset;

    _spinAngle += _spinSpeed * deltaTime;

    transform.rotation = Quaternion.Euler(
      0,
      0,
      _angle + headingOffset - _rotation + _spinAngle
    );
  }

  private static bool SharesGroup(
    List<Constants.CollisionGroups> groups,
    List<Constants.CollisionGroups> other
  )
  {
    for (int i = 0; i < groups.Count; i++)
    {
      if (other.Contains(groups[i])) return true;
    }
    return false;
  }

  private void OnTriggerEnter2D(Collider2D collider)
  {
    if (!collider) return;

    var collidableComponent = collider.gameObject.GetComponent<Collidable>();
    if (!collidableComponent) return;

    if (SharesGroup(collidableComponent.collisionGroups, _willDamage))
    {
      collider.SendMessage(
        Constants.NPCMessages.ReceiveDamage,
        new DamageModel(_damage, _statusEffects)
      );
    }

    if (SharesGroup(collidableComponent.collisionGroups, _willPenetrate))
    {
      return;
    }

    if (collider.name != "BulletCollision")
    {
      ParticleManager.Singleton.SpawnParticles(transform, _particleColor, 6);
      if (collider.name == "Collision" && !SilentWallHits)
      {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.HitWall);
      }
    }

    Destroy(gameObject);
  }
}