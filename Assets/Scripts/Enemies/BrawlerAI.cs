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
    
    [Header("Debug Visuals")]
    [SerializeField] private bool showRanges = true;

    private LineRenderer _searchRangeCircle;
    private LineRenderer _attackEnterRangeCircle;
    private LineRenderer _attackExitRangeCircle;

    [Header("Ranges")]
    [SerializeField] private float SearchRadius = 5f;
    [SerializeField] private float AttackEnterRange = 4f;

    private readonly float _shootingDelay = 0.8f;
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

            _attackEnterRangeCircle = Utils.CreateCircle(
                transform,
                "AttackEnterRange",
                AttackEnterRange,
                new Color(1f, 1f, 0f, 0.4f)
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
        return Time.time - _lastFired > _shootingDelay;
    }

    void ShootPlayer()
    {
        if (!CanShoot()) return;
        
        _isShooting = true;
        _animator.SetTrigger(Attack);

        var shootDirection = (Utils.GetPlayerPosition() - transform.position).normalized;

        var proj = Instantiate(
            _projectile,
            transform.position,
            Quaternion.identity
        );

        proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
            shootDirection,
            0,
            null,
            null,
            10,
            ResourceCacher.Singleton.ProjectileSprites[17],
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
            null
        ));

        _lastFired = Time.time;
        Invoke(nameof(EndShootPause), 0.15f);
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
