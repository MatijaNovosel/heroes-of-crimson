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
    [SerializeField] private float AttackExitRange = 4.8f;

    private readonly float _shootingDelay = 0.8f;
    private float _lastFired;

    private float wanderRadius = 1f;
    private float wanderDuration = 0.2f;
    private float wanderPause = 2f;

    private Vector2 _wanderDirection;
    private float _wanderTimer;
    private float _wanderPauseTimer;
    
    private float combatMoveDuration = 1f;
    private float combatPause = 1f;
    private Vector2 _combatMoveDirection;
    private float _combatMoveTimer;
    
    private bool _inAttackRange;
    private Vector2 _combatTarget;

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
                AttackExitRange,
                new Color(0.1f, 1f, 0f, 0.5f)
            );
        }
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _npcBehaviour = GetComponent<BaseNPCBehaviour>();
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
        if (Utils.IsPlayerDead())
        {
            Wander();
            return;
        }

        HandleFlipping();
        HandleMovementAndAttack();
    }
    
    private void CombatMove()
    {
        _combatMoveTimer -= Time.deltaTime;

        if (_combatMoveTimer <= 0f)
        {
            PickNewCombatTarget();
            return;
        }

        var dir = (_combatTarget - (Vector2)transform.position).normalized;
        _npcBehaviour.Move(dir);
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

        if (!_inAttackRange && distance <= AttackEnterRange)
        {
            _inAttackRange = true;
            PickNewCombatTarget();
        }
        else if (_inAttackRange && distance >= AttackExitRange)
        {
            _inAttackRange = false;
        }

        if (!_inAttackRange)
        {
            MoveTowards(playerPos);
            return;
        }

        CombatMove();
        ShootPlayer();
    }

    
    private void MoveTowards(Vector3 target)
    {
        var direction = (target - transform.position).normalized;
        _npcBehaviour.Move(direction);
    }

    private void Wander()
    {
        // Pause between wanders
        if (_wanderPauseTimer > 0)
        {
            _wanderPauseTimer -= Time.deltaTime;
            return;
        }

        // Wander in chosen direction
        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0)
        {
            PickNewWanderDirection();
            return;
        }

        _npcBehaviour.Move(_wanderDirection);
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

        var shootDirection = (Utils.GetPlayerPosition() - transform.position).normalized;

        var proj = Instantiate(
            _projectile,
            new Vector3(transform.position.x, transform.position.y, 0),
            Quaternion.identity
        );

        proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
            shootDirection,
            0,
            null,
            null,
            50,
            ResourceCacher.Singleton.ProjectileSprites[17],
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player },
            new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy }
        ));

        _lastFired = Time.time;
    }

    private void HandleFlipping()
    {
        var targetX = Utils.IsPlayerDead()
            ? transform.position.x + _wanderDirection.x
            : Utils.GetPlayerPosition().x;

        _spriteRenderer.flipX = targetX < transform.position.x;
    }
}
