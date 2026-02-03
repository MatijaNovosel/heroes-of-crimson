using System.Collections.Generic;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class KrakenTentacleOrbit2D : MonoBehaviour
{
    private GameObject _kraken;
    public float radius = 3.5f;
    public float angularSpeed = 20f;
    public bool clockwise = false;

    public bool startFromCurrent = true;
    public float initialAngleDeg = 0f;

    private float _angleDeg;
    
    private readonly float _shootingDelay = 2f;
    private float _lastFired;
    
    [Header("Combat")]
    public float attackRange = 8f;

    // Components
    private BaseNPCBehaviour _baseNpcBehaviour;
    private Rigidbody2D _rigidBody;
    private GameObject _projectile;
    public SpriteRenderer spriteRenderer;
    
    private bool IsPlayerInRange()
    {
        Vector2 playerPos = Utils.GetPlayerPosition();
        return Vector2.SqrMagnitude(playerPos - (Vector2)transform.position) 
               <= attackRange * attackRange;
    }
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _projectile = Resources.Load<GameObject>("Prefabs/Projectile");
        
        var go = GameObject.Find("Kraken");
        if (go) _kraken = go;

        Vector2 center = _kraken.transform.position;

        if (startFromCurrent)
        {
            var dir = (Vector2)transform.position - center;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right * Mathf.Max(0.001f, radius);
            if (radius <= 0.001f) radius = dir.magnitude;
            _angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            _angleDeg = initialAngleDeg;
            SetPosition(center + Dir(_angleDeg) * radius);
        }
    }

    private void Update()
    {
        if (!_kraken) Destroy(gameObject);
        Orbit(Time.deltaTime);
        ShootPlayer();
    }

    private bool CanFire()
    {
        // Current game time in seconds - last time fired in game seconds
        return Time.time - _lastFired > _shootingDelay;
    }
    
    private void ShootPlayer()
    {
        if (!CanFire() || Utils.IsPlayerDead() || !IsPlayerInRange()) return;
        
        var shootDirection = (Utils.GetPlayerPosition() - gameObject.transform.position).normalized;
        
        var proj = Instantiate(
            _projectile,
            new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0),
            Quaternion.identity
        );

        proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
            shootDirection,
            45f, 
            null,
            null,
            50,
            ResourceCacher.Singleton.ProjectileSprites[22],
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
            Color.purple,
            new (),
            5.0f
        ));
        
        _lastFired = Time.time;
    }

    private void Orbit(float dt)
    {
        if (!_kraken) return;
        
        var sign = clockwise ? -1f : 1f;
        _angleDeg = Mathf.Repeat(_angleDeg + sign * angularSpeed * dt, 360f);

        Vector2 center = _kraken.transform.position;
        var pos = center + Dir(_angleDeg) * radius;

        SetPosition(pos);

        if (IsPlayerInRange())
        {
            var playerDir = Utils.GetPlayerPosition() - transform.position;
            spriteRenderer.flipX = playerDir.x > 0;
        }
    }

    private static Vector2 Dir(float deg)
    {
        var r = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    private void SetPosition(Vector2 pos)
    {
        if (_rigidBody) _rigidBody.MovePosition(pos);
        else transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}