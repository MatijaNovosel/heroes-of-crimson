using System.Collections.Generic;
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

    // Components
    private BaseNPCBehaviour _baseNpcBehaviour;
    private Rigidbody2D _rigidBody;
    private Sprite _sprite;
    private GameObject _projectile;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _projectile = Resources.Load<GameObject>("Prefabs/Projectile");
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
        _sprite = sprites[1];
    }

    void Start()
    {
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

    void Update()
    {
        if (!_kraken) Destroy(gameObject);
        Orbit(Time.deltaTime);
        ShootPlayer();
    }

    bool CanFire()
    {
        // Current game time in seconds - last time fired in game seconds
        return Time.time - _lastFired > _shootingDelay;
    }
    
    private void ShootPlayer()
    {
        if (!CanFire() || Utils.IsPlayerDead()) return;
        
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
            _sprite,
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy }
        ));
        
        _lastFired = Time.time;
    }

    void Orbit(float dt)
    {
        if (!_kraken) return;
        
        var sign = clockwise ? -1f : 1f;
        _angleDeg = Mathf.Repeat(_angleDeg + sign * angularSpeed * dt, 360f);

        Vector2 center = _kraken.transform.position;
        var pos = center + Dir(_angleDeg) * radius;

        SetPosition(pos);

        // Flip when on the right half of the circle
        var onRight = pos.x > center.x;
        spriteRenderer.flipX = onRight;
    }

    static Vector2 Dir(float deg)
    {
        var r = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    void SetPosition(Vector2 pos)
    {
        if (_rigidBody) _rigidBody.MovePosition(pos);
        else transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}