using System.Collections.Generic;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class BrawlerAI : MonoBehaviour
{
    private BaseNPCBehaviour _npcBehaviour;
    private SpriteRenderer _spriteRenderer;
    private GameObject _projectile;

    // Shared, never mutated by Projectile
    private static readonly List<Constants.CollisionGroups> DamagesPlayer = new() { Constants.CollisionGroups.Player };
    private static readonly List<Constants.CollisionGroups> PenetratesEnemies = new() { Constants.CollisionGroups.Enemy };

    [Header("Projectile data")]
    public float projectileScale = 1f;
    public Sprite projectileSprite;
    public Constants.StatusEffects? StatusEffectToApply;
    
    [Header("Debug Visuals")]
    [SerializeField] private bool showRanges = true;

    private LineRenderer _searchRangeCircle;
    private LineRenderer _attackExitRangeCircle;

    [Header("Ranges")]
    [SerializeField] private float SearchRadius = 5f;
    [SerializeField] private float AttackEnterRange = 4f;

    [Header("Shooting")]
    [SerializeField] protected float shootingDelay = 0.8f;
    [SerializeField] protected float projectileDamage = 10f;
    private float _lastFired;

    private float wanderDuration = 0.2f;
    private float wanderPause = 2f;

    private Vector2 _wanderDirection;
    private float _wanderTimer;
    private float _wanderPauseTimer;
    
    private float combatMoveDuration = 1f;
    private Vector2 _combatMoveDirection;
    private float _combatMoveTimer;
    
    private bool _inAttackRange;
    private Vector2 _combatTarget;

    private const float CombatArriveRadius = 0.15f;
    private bool _isShooting;
    
    private float combatSpeedMultiplier = 0.4f;
    private float wanderSpeedMultiplier = 0.6f;
    
    private static readonly int Attacking = Animator.StringToHash("Attacking");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private Vector2 _lastPosition;
    private float _animSpeed;
    
    private Animator _animator;

    void Start()
    {
        if (showRanges)
        {
            _searchRangeCircle = Utils.CreateCircle(
                transform,
                "SearchRange",
                SearchRadius,
                new Color(1f, 0f, 0f, 0.25f)
            );
            
            _attackExitRangeCircle = Utils.CreateCircle(
                transform,
                "AttackExitRangeCircle",
                AttackEnterRange + 0.8f,
                new Color(0.1f, 1f, 0f, 0.5f)
            );
        }
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _npcBehaviour = GetComponent<BaseNPCBehaviour>();
        _animator = GetComponent<Animator>();
        _projectile = Resources.Load<GameObject>("Prefabs/Projectile");

        PickNewWanderDirection();
    }
    
    private void PickNewCombatTarget()
    {
        var playerPos = Utils.GetPlayerPosition();
        
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float safeRadius = AttackEnterRange * 0.85f;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * safeRadius;

        _combatTarget = (Vector2)playerPos + offset;
        _combatMoveTimer = combatMoveDuration;
    }

    void FixedUpdate()
    {
        _animSpeed = 0f;
        
        if (Utils.IsPlayerDead())
        {
            Wander();
            return;
        }

        HandleFlipping();
        HandleMovementAndAttack();
        _animator.SetFloat("Speed", _animSpeed);
    }
    
    private void CombatMove()
    {
        _combatMoveTimer -= Time.deltaTime;

        var toTarget = _combatTarget - (Vector2)transform.position;
        var distance = toTarget.magnitude;

        if (distance < CombatArriveRadius || _combatMoveTimer <= 0f)
        {
            PickNewCombatTarget();
            return;
        }

        var dir = toTarget / distance;
        _npcBehaviour.Move(dir * combatSpeedMultiplier);
        _animSpeed = combatSpeedMultiplier;
    }


    private void HandleMovementAndAttack()
    {
        var playerPos = Utils.GetPlayerPosition();
        var distance = Vector2.Distance(playerPos, transform.position);

        if (distance > SearchRadius)
        {
            Wander();
            return;
        }

        _wanderTimer = 0;
        _wanderPauseTimer = 0;

        var attackExitRange = AttackEnterRange + 0.8f;

        if (!_inAttackRange && distance <= AttackEnterRange)
        {
            _inAttackRange = true;
            PickNewCombatTarget();
        }
        else if (_inAttackRange && distance >= attackExitRange)
        {
            _inAttackRange = false;
        }

        if (_inAttackRange)
        {
            if (!_isShooting) CombatMove();
            ShootPlayer();
        }
        else
        {
            _animator.SetBool(Attacking, false);
            MoveTowards(playerPos);
        }
    }
    
    private void MoveTowards(Vector3 target)
    {
        var dir = (target - transform.position).normalized;
        _npcBehaviour.Move(dir);
        _animSpeed = 1f;
    }

    private void Wander()
    {
        if (_wanderPauseTimer > 0)
        {
            _wanderPauseTimer -= Time.deltaTime;
            return;
        }

        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0)
        {
            PickNewWanderDirection();
            return;
        }

        _npcBehaviour.Move(_wanderDirection * wanderSpeedMultiplier);
        _animSpeed = wanderSpeedMultiplier;
    }

    private void PickNewWanderDirection()
    {
        _wanderDirection = Random.insideUnitCircle.normalized;
        _wanderTimer = wanderDuration;
        _wanderPauseTimer = wanderPause;
    }
    
    private bool CanShoot()
    {
        return Time.time - _lastFired > shootingDelay;
    }

    void ShootPlayer()
    {
        if (!CanShoot()) return;
        
        _isShooting = true;
        _animator.SetTrigger(Attack);

        var shootDirection = (Utils.GetPlayerPosition() - transform.position).normalized;
        FireAtPlayer(shootDirection);

        _lastFired = Time.time;
        Invoke(nameof(EndShootPause), 0.15f);
    }

    /// <summary>
    /// Spawns the attack's projectiles. Override to change the attack pattern.
    /// </summary>
    protected virtual void FireAtPlayer(Vector3 shootDirection)
    {
        var statusEffects = new List<Constants.StatusEffects>();

        if (StatusEffectToApply != null)
        {
            statusEffects.Add((Constants.StatusEffects)StatusEffectToApply);
        }

        SpawnProjectile(shootDirection, projectileDamage, statusEffects);
    }

    protected void SpawnProjectile(Vector3 direction, float damage, List<Constants.StatusEffects> statusEffects)
    {
        var proj = Instantiate(
            _projectile,
            transform.position,
            Quaternion.identity
        );

        proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
            direction,
            0,
            null,
            projectileScale,
            damage,
            projectileSprite,
            DamagesPlayer,
            PenetratesEnemies,
            null,
            statusEffects,
            5.0f,
            null,
            null
        ));
    }

    void EndShootPause()
    {
        _isShooting = false;
    }

    private void HandleFlipping()
    {
        var playerPos = Utils.GetPlayerPosition();
        if (Vector2.Distance(playerPos, transform.position) > SearchRadius) return;
        _spriteRenderer.flipX = playerPos.x < transform.position.x;
    }
}
