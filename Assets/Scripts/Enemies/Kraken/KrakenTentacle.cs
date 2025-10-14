using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class KrakenTentacleOrbit2D : MonoBehaviour
{
    private BaseNPCBehaviour baseNPCBehaviour;
    
    private GameObject kraken;
    public float radius = 3.5f;
    public float angularSpeed = 20f;
    public bool clockwise = false;

    public bool startFromCurrent = true;
    public float initialAngleDeg = 0f;

    public SpriteRenderer spriteRenderer;

    private float angleDeg;
    private Rigidbody2D rb;
    
    private readonly float shootingDelay = 2f; // 0.8f -> 200 ms
    private float lastFired;

    private Sprite sprite;
    private GameObject projectile;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectile = Resources.Load<GameObject>("Prefabs/Projectile");
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/genericProjectiles");
        sprite = sprites[1];
    }

    void Start()
    {
        var go = GameObject.Find("Kraken");
        if (go) kraken = go;

        Vector2 center = kraken.transform.position;

        if (startFromCurrent)
        {
            var dir = (Vector2)transform.position - center;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right * Mathf.Max(0.001f, radius);
            if (radius <= 0.001f) radius = dir.magnitude;
            angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            angleDeg = initialAngleDeg;
            SetPosition(center + Dir(angleDeg) * radius);
        }
    }

    void Update()
    {
        Orbit(Time.deltaTime);
        ShootPlayer();
    }

    bool CanFire()
    {
        // Current game time in seconds - last time fired in game seconds
        return Time.time - lastFired > shootingDelay;
    }
    
    private void ShootPlayer()
    {
        if (!CanFire()) return;
        
        var shootDirection = (Utils.GetPlayerPosition() - gameObject.transform.position).normalized;
        
        
        var proj = Instantiate(
            projectile,
            new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0),
            Quaternion.identity
        );

        proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
            shootDirection,
            45f, 
            null, 
            gameObject,
            sprite
        ));
        
        lastFired = Time.time;
    }

    void Orbit(float dt)
    {
        if (!kraken) return;
        
        var sign = clockwise ? -1f : 1f;
        angleDeg = Mathf.Repeat(angleDeg + sign * angularSpeed * dt, 360f);

        Vector2 center = kraken.transform.position;
        var pos = center + Dir(angleDeg) * radius;

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
        if (rb) rb.MovePosition(pos);
        else transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}