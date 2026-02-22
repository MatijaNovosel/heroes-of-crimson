using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class BomberAI : MonoBehaviour
{
    private BaseNPCBehaviour _npcBehaviour;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    [Header("Projectile Visual (sprite-only)")]
    public Sprite projectileSprite;
    public float projectileScale = 1f;
    [SerializeField] private float projectileZ = -0.2f;
    [SerializeField] private float spinDegreesPerSecond = 720f;
    [SerializeField] private float endScaleMultiplier = 0.25f;
    [SerializeField] private Color particleColor = Color.green;
    [SerializeField] private int particleCount = 18;

    [SerializeField] private float arcHeightMultiplier = 0.35f;
    [SerializeField] private float minArcHeight = 0.4f;
    [SerializeField] private float maxArcHeight = 2.5f;

    [SerializeField] private float AttackEnterRange = 5f;
    [SerializeField] private float AttackExitRangePadding = 1f;
    [SerializeField] private float bombCooldown = 2.4f;

    [SerializeField] private float flightTime = 0.9f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private bool predictPlayer = true;
    [SerializeField] private float predictionLeadSeconds = 0.35f;
    [SerializeField] private bool showRanges = true;

    private LineRenderer _searchRangeCircle;
    private LineRenderer _attackRangeCircle;

    [SerializeField] private float SearchRadius = 7f;

    private float wanderDuration = 0.2f;
    private float wanderPause = 2f;
    private Vector2 _wanderDirection;
    private float _wanderTimer;
    private float _wanderPauseTimer;

    private bool _inAttackRange;
    private bool _isAttacking;
    private float _lastBombTime;

    private Vector2 _lastPlayerPos;
    private Vector2 _playerVelEstimate;
    private bool _hasLastPlayerPos;
    [SerializeField] private float playerVelSmoothing = 0.25f;

    private float _animSpeed;

    private readonly List<GameObject> _activeIndicators = new();
    private readonly List<GameObject> _activeProjectiles = new();

    private static readonly int Attacking = Animator.StringToHash("Attacking");
    private static readonly int Attack = Animator.StringToHash("Attack");

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _npcBehaviour = GetComponent<BaseNPCBehaviour>();
        _animator = GetComponent<Animator>();

        if (showRanges)
        {
            _searchRangeCircle = Utils.CreateCircle(
                transform,
                "SearchRange",
                SearchRadius,
                new Color(1f, 0f, 0f, 0.25f)
            );

            _attackRangeCircle = Utils.CreateCircle(
                transform,
                "AttackRange",
                AttackEnterRange,
                new Color(0.1f, 1f, 0f, 0.35f)
            );
        }

        PickNewWanderDirection();
        UpdatePlayerTracking();
    }

    void FixedUpdate()
    {
        _animSpeed = 0f;

        if (Utils.IsPlayerDead())
        {
            Wander();
            _animator.SetFloat("Speed", _animSpeed);
            return;
        }

        UpdatePlayerTracking();
        HandleFlipping();
        HandleMovementAndAttack();

        _animator.SetFloat("Speed", _animSpeed);
    }

    private void UpdatePlayerTracking()
    {
        if (Utils.IsPlayerDead()) return;

        var current = (Vector2)Utils.GetPlayerPosition();
        var dt = Time.fixedDeltaTime;
        if (dt <= 0f) return;

        if (!_hasLastPlayerPos)
        {
            _hasLastPlayerPos = true;
            _lastPlayerPos = current;
            _playerVelEstimate = Vector2.zero;
            return;
        }

        var rawVel = (current - _lastPlayerPos) / dt;
        _playerVelEstimate = Vector2.Lerp(_playerVelEstimate, rawVel, Mathf.Clamp01(playerVelSmoothing));
        _lastPlayerPos = current;
    }

    private void HandleMovementAndAttack()
    {
        var playerPos = Utils.GetPlayerPosition();
        var distance = Vector2.Distance(playerPos, transform.position);

        if (distance > SearchRadius)
        {
            _animator.SetBool(Attacking, false);
            Wander();
            return;
        }

        _wanderTimer = 0;
        _wanderPauseTimer = 0;

        var exitRange = AttackEnterRange + AttackExitRangePadding;

        if (!_inAttackRange && distance <= AttackEnterRange)
        {
            _inAttackRange = true;
        }
        else if (_inAttackRange && distance >= exitRange)
        {
            _inAttackRange = false;
        }

        if (_inAttackRange)
        {
            _animator.SetBool(Attacking, true);

            if (!_isAttacking)
            {
                if (distance < AttackEnterRange * 0.55f)
                {
                    var away = ((Vector2)transform.position - (Vector2)playerPos).normalized;
                    _npcBehaviour.Move(away * 0.35f);
                    _animSpeed = 0.35f;
                }
                else
                {
                    var toPlayer = ((Vector2)playerPos - (Vector2)transform.position).normalized;
                    var strafe = new Vector2(-toPlayer.y, toPlayer.x);
                    _npcBehaviour.Move(strafe * 0.25f);
                    _animSpeed = 0.25f;
                }
            }

            TryBomb(playerPos);
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
        if (_wanderPauseTimer > 0f)
        {
            _wanderPauseTimer -= Time.deltaTime;
            return;
        }

        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0f)
        {
            PickNewWanderDirection();
            return;
        }

        _npcBehaviour.Move(_wanderDirection * 0.6f);
        _animSpeed = 0.6f;
    }

    private void PickNewWanderDirection()
    {
        _wanderDirection = Random.insideUnitCircle.normalized;
        _wanderTimer = wanderDuration;
        _wanderPauseTimer = wanderPause;
    }

    private bool CanBomb()
    {
        return Time.time - _lastBombTime > bombCooldown;
    }

    private Vector2 GetPlayerVelocityEstimate() => _playerVelEstimate;

    private Vector2 ComputeTargetPoint(Vector2 playerPosNow)
    {
        if (!predictPlayer) return playerPosNow;
        var v = GetPlayerVelocityEstimate();
        return playerPosNow + v * predictionLeadSeconds;
    }

    private void TryBomb(Vector2 playerPosNow)
    {
        if (!CanBomb() || _isAttacking) return;

        _isAttacking = true;
        _animator.SetTrigger(Attack);

        var targetPoint = ComputeTargetPoint(playerPosNow);

        var indicator = CreateLandingIndicator(targetPoint, explosionRadius);
        if (indicator) _activeIndicators.Add(indicator);

        var projGo = CreateVisualProjectile((Vector2)transform.position, projectileSprite, projectileScale);
        if (projGo != null)
        {
            _activeProjectiles.Add(projGo);
            StartCoroutine(ArcMove(projGo.transform, (Vector2)transform.position, targetPoint, flightTime));
        }

        StartCoroutine(DelayedExplosion(targetPoint, flightTime, explosionRadius, indicator));
        _lastBombTime = Time.time;
        Invoke(nameof(EndAttackPause), 0.18f);
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.SpiderShoot);
    }

    private void EndAttackPause()
    {
        _isAttacking = false;
    }

    private System.Collections.IEnumerator DelayedExplosion(Vector2 worldPos, float delay, float radius, GameObject indicator)
    {
        yield return new WaitForSeconds(delay);

        if (indicator)
        {
            Destroy(indicator);
            _activeIndicators.Remove(indicator);
        }

        if (!this || !isActiveAndEnabled) yield break;

        var hits = Physics2D.OverlapCircleAll(worldPos, radius);

        foreach (var col in hits)
        {
            if (!col) continue;

            var collidable = col.GetComponent<Collidable>();
            if (!collidable) continue;

            if (collidable.collisionGroups.All(x => x != Constants.CollisionGroups.Player)) continue;

            col.SendMessage(
                "ReceiveDamage",
                new DamageModel(
                    10f,
                    new List<Constants.StatusEffects>() { Constants.StatusEffects.Poisoned }
                ),
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    private GameObject CreateVisualProjectile(Vector2 startPos, Sprite sprite, float scale)
    {
        if (!sprite) return null;

        var go = new GameObject("BombProjectile");
        go.transform.position = new Vector3(startPos.x, startPos.y, projectileZ);
        go.transform.localScale = Vector3.one * Mathf.Max(0.0001f, scale);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 9999;

        if (_spriteRenderer) sr.flipX = _spriteRenderer.flipX;

        return go;
    }

    private System.Collections.IEnumerator ArcMove(Transform tr, Vector2 start, Vector2 end, float time)
    {
        if (!tr) yield break;

        float elapsed = 0f;

        float dist = Vector2.Distance(start, end);
        float height = Mathf.Clamp(dist * arcHeightMultiplier, minArcHeight, maxArcHeight);

        float z = projectileZ;

        Vector3 startScale = tr.localScale;
        Vector3 endScale = startScale * Mathf.Clamp(endScaleMultiplier, 0.01f, 1f);

        while (elapsed < time)
        {
            if (!tr) yield break;

            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / time);

            var pos = Vector2.Lerp(start, end, u);
            float arc = 4f * height * u * (1f - u);

            tr.position = new Vector3(pos.x, pos.y + arc, z);
            tr.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
            tr.localScale = Vector3.Lerp(startScale, endScale, u);

            yield return null;
        }

        if (tr) tr.position = new Vector3(end.x, end.y, z); 
        ParticleManager.Singleton.SpawnParticles(tr, particleColor, particleCount);

        if (tr)
        {
            var go = tr.gameObject;
            _activeProjectiles.Remove(go);
            Destroy(go);
        }
    }

    private GameObject CreateLandingIndicator(Vector2 center, float radius)
    {
        var root = new GameObject("BombLandingIndicator");
        root.transform.position = new Vector3(center.x, center.y, -0.1f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(root.transform, false);

        var meshFilter = fillGO.AddComponent<MeshFilter>();
        var meshRenderer = fillGO.AddComponent<MeshRenderer>();

        var fillShader = Shader.Find("Sprites/Default");
        meshRenderer.material = new Material(fillShader);
        meshRenderer.material.color = new Color(1f, 0f, 0f, 0.25f);
        meshRenderer.sortingLayerName = "Actor";
        meshRenderer.sortingOrder = 1999;

        meshFilter.mesh = CreateCircleMesh(radius, 64);

        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(root.transform, false);

        var lr = borderGO.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;

        int segments = 96;
        lr.positionCount = segments;

        lr.startWidth = 0.12f;
        lr.endWidth = 0.12f;

        var borderShader = Shader.Find("Sprites/Default");
        lr.material = new Material(borderShader);
        lr.material.color = new Color(1f, 0f, 0f, 1f);

        lr.sortingLayerName = "Actor";
        lr.sortingOrder = 2000;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            float ang = t * Mathf.PI * 2f;

            lr.SetPosition(i, new Vector3(
                Mathf.Cos(ang) * radius,
                Mathf.Sin(ang) * radius,
                0f
            ));
        }

        return root;
    }

    private Mesh CreateCircleMesh(float radius, int segments)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void HandleFlipping()
    {
        var playerPos = Utils.GetPlayerPosition();
        if (Vector2.Distance(playerPos, transform.position) > SearchRadius) return;
        _spriteRenderer.flipX = playerPos.x < transform.position.x;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        CleanupIndicators();
        CleanupProjectiles();
    }

    private void OnDestroy()
    {
        CleanupIndicators();
        CleanupProjectiles();
    }

    private void CleanupIndicators()
    {
        for (int i = _activeIndicators.Count - 1; i >= 0; i--)
        {
            var ind = _activeIndicators[i];
            if (ind) Destroy(ind);
        }
        _activeIndicators.Clear();
    }

    private void CleanupProjectiles()
    {
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            var p = _activeProjectiles[i];
            if (p) Destroy(p);
        }
        _activeProjectiles.Clear();
    }
}