using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using TMPro;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    private enum Phase
    {
        Dormant = 0,
        Waves = 1,
        ReverseWaves = 2,
        Summoner = 3,
        Chaser = 4
    }
    
    [SerializeField] private float aggroRadius = 7f;

    [SerializeField] private float projectileRange = 14f;
    [SerializeField] private float projectileRotationOffset = 0f;

    [Header("Phase 1 & 2: Rotating Waves")]
    [SerializeField] private Sprite waveProjectileSprite;
    [SerializeField] private int waveArms = 4;
    [SerializeField] private float waveDamage = 50f;
    [SerializeField] private float waveProjectileSpeed = 5f;
    [SerializeField] private float waveProjectileScale = 1f;
    [SerializeField] private float waveFireInterval = 0.15f;
    [SerializeField] private float waveRotationSpeed = 35f;

    [Header("Phase 1, 2 & 4: Aimed Shot")]
    [SerializeField] private Sprite aimedProjectileSprite;
    [SerializeField] private float aimedDamage = 20f;
    [SerializeField] private float aimedProjectileSpeed = 8f;
    [SerializeField] private float aimedProjectileScale = 1f;
    [SerializeField] private float aimedShotMinInterval = 2f;
    [SerializeField] private float aimedShotMaxInterval = 4f;
    [SerializeField] private Color aimedParticleColor = Color.red;

    [Header("Phase 3: Minions")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionCount = 4;
    [SerializeField] private float minionSpawnDistance = 1.75f;
    [SerializeField] private float resummonInterval = 5f;

    [Header("Phase 4: Chase & Bombs")]
    [SerializeField] private float chaseSpeedMultiplier = 0.45f;
    [SerializeField] private float chaseStopDistance = 1.5f;
    [SerializeField] private float bombInterval = 2.2f;
    [SerializeField] private float bombLeadSeconds = 0.35f;

    [Header("Phase Switch Nova")]
    [SerializeField] private Sprite novaProjectileSprite;
    [SerializeField] private float novaDamage = 15f;
    [SerializeField] private float novaProjectileSpeed = 7f;
    [SerializeField] private float novaProjectileScale = 1f;
    [SerializeField] private float novaCoverRadius = 10f;
    [SerializeField] private float novaMaxGap = 1.1f;
    [SerializeField] private Color novaParticleColor = new(1f, 0.5f, 0f);
    [SerializeField] private bool novaOnFightStart = false;
    [SerializeField] private float phaseSwitchPause = 1f;

    [Header("Health Bar")]
    [SerializeField] private string bossName = "The Lich";
    [Tooltip("Jersey SDF, to match the rest of the HUD.")]
    [SerializeField] private TMP_FontAsset healthBarFont;

    private static readonly float[] PhaseLowerBound = { 1f, 0.75f, 0.5f, 0.25f, 0f };

    private static readonly List<Constants.CollisionGroups> DamagesPlayer = new() { Constants.CollisionGroups.Player };
    private static readonly List<Constants.CollisionGroups> PenetratesEnemies = new() { Constants.CollisionGroups.Enemy };
    private static readonly List<Constants.StatusEffects> NoStatusEffects = new();
    private static readonly List<Constants.StatusEffects> AimedShotStatusEffects = new() { Constants.StatusEffects.Paralyzed };

    private BaseNPCBehaviour _npc;
    private BombLobber _bombLobber;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private GameObject _projectilePrefab;

    private Phase _phase = Phase.Dormant;
    private float _attacksResumeTime;

    // Waves
    private float _waveAngle;
    private float _waveTimer;
    private float _nextAimedShotTime;

    // Minions
    private readonly List<GameObject> _minions = new();
    private float _resummonTimer;
    private bool _isInvincible;

    // Chase
    private float _nextBombTime;
    private Vector2 _lastPlayerPos;
    private Vector2 _playerVelocity;
    private bool _hasLastPlayerPos;

    // Animator
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int AttackParam = Animator.StringToHash("Attack");
    private bool _hasSpeedParam;
    private bool _hasAttackParam;

    private void Start()
    {
        _npc = GetComponent<BaseNPCBehaviour>();
        _bombLobber = GetComponent<BombLobber>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");

        if (!minionPrefab) minionPrefab = Resources.Load<GameObject>("Prefabs/Enemies/Skeleton");

        /*
        if (_animator && _animator.runtimeAnimatorController)
        {
            foreach (var p in _animator.parameters)
            {
                if (p.nameHash == SpeedParam) _hasSpeedParam = true;
                if (p.nameHash == AttackParam) _hasAttackParam = true;
            }
        }
        */
    }

    private void Update()
    {
        if (!TryGetPlayerPosition(out var playerPos)) return;

        if (_phase == Phase.Dormant)
        {
            // Wake up when the player gets close, or if shot from outside the aggro radius
            bool playerClose = Vector2.Distance(transform.position, playerPos) <= aggroRadius;
            bool wasHit = _npc.hp < _npc.maxHp;
            if (!playerClose && !wasHit) return;
            EnterPhase(Phase.Waves, novaOnFightStart);
        }

        CheckPhaseTransition();
        FacePlayer(playerPos);
        TrackPlayerVelocity(playerPos);

        if (_phase == Phase.Summoner) UpdateMinions();

        if (Time.time < _attacksResumeTime) return;

        switch (_phase)
        {
            case Phase.Waves:
                UpdateWaves(-1f); // clockwise
                UpdateAimedShot(playerPos);
                break;
            case Phase.ReverseWaves:
                UpdateWaves(1f); // counter-clockwise
                UpdateAimedShot(playerPos);
                break;
            case Phase.Chaser:
                UpdateBombs(playerPos);
                UpdateAimedShot(playerPos);
                break;
        }
    }

    private void FixedUpdate()
    {
        float animSpeed = 0f;

        if (_phase == Phase.Chaser && Time.time >= _attacksResumeTime && TryGetPlayerPosition(out var playerPos))
        {
            var toPlayer = playerPos - (Vector2)transform.position;
            if (toPlayer.magnitude > chaseStopDistance)
            {
                _npc.Move(toPlayer.normalized * chaseSpeedMultiplier);
                animSpeed = chaseSpeedMultiplier;
            }
        }

        // if (_hasSpeedParam) _animator.SetFloat(SpeedParam, animSpeed);
    }

    private void CheckPhaseTransition()
    {
        if (_phase == Phase.Chaser) return;

        float hpFraction = _npc.hp / _npc.maxHp;
        if (hpFraction <= PhaseLowerBound[(int)_phase])
        {
            EnterPhase(_phase + 1, true);
        }
    }

    private void EnterPhase(Phase phase, bool fireNova)
    {
        _phase = phase;
        
        switch (phase)
        {
            case Phase.Waves:
                BossHealthBar.Create(_npc, bossName, healthBarFont);
                Shout("The bell tolls for <color=#E74C3C>YOU</color>, mortal!");
                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.LichIntro);
                break;
            case Phase.ReverseWaves:
                Shout("Your efforts are futile, give up!");
                break; 
            case Phase.Summoner:
                Shout("NO! I shan't be cast down by a beggar like you!");
                break;
            case Phase.Chaser:
                Shout("DIE!!!");
                break;
        }

        if (phase != Phase.Waves)
        {
            float floor = _npc.maxHp * PhaseLowerBound[(int)phase];
            _npc.hp = Mathf.Max(_npc.hp, floor + 1f);
        }

        if (fireNova)
        {
            FireNova();
            _attacksResumeTime = Time.time + phaseSwitchPause;
        }

        _waveTimer = 0f;
        _nextAimedShotTime = Time.time + phaseSwitchPause + Random.Range(aimedShotMinInterval, aimedShotMaxInterval);
        _nextBombTime = Time.time + phaseSwitchPause + bombInterval * 0.5f;

        switch (phase)
        {
            case Phase.Summoner:
                SummonMinions(minionCount);
                _resummonTimer = resummonInterval;
                break;
            case Phase.Chaser:
                SetInvincible(false);
                break;
        }
    }

    private void UpdateWaves(float rotationDirection)
    {
        _waveAngle += rotationDirection * waveRotationSpeed * Time.deltaTime;
        _waveTimer -= Time.deltaTime;

        while (_waveTimer <= 0f)
        {
            _waveTimer += waveFireInterval;

            float step = 360f / Mathf.Max(1, waveArms);
            for (int i = 0; i < waveArms; i++)
            {
                FireProjectile(
                    AngleToDirection(_waveAngle + i * step),
                    waveProjectileSprite,
                    waveProjectileSpeed,
                    waveProjectileScale,
                    waveDamage,
                    Color.white,
                    NoStatusEffects
                );
            }
        }
    }

    private void UpdateAimedShot(Vector2 playerPos)
    {
        if (Time.time < _nextAimedShotTime) return;
        _nextAimedShotTime = Time.time + Random.Range(aimedShotMinInterval, aimedShotMaxInterval);

        var direction = (playerPos - (Vector2)transform.position).normalized;
        FireProjectile(
            direction, 
            aimedProjectileSprite,
            aimedProjectileSpeed,
            aimedProjectileScale,
            aimedDamage,
            aimedParticleColor,
            AimedShotStatusEffects
        );

        // if (_hasAttackParam) _animator.SetTrigger(AttackParam);
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.MagicShoot);
    }

    private void UpdateMinions()
    {
        _minions.RemoveAll(m => m == null);

        _resummonTimer -= Time.deltaTime;
        if (_resummonTimer <= 0f)
        {
            _resummonTimer = resummonInterval;
            if (_minions.Count < minionCount) SummonMinions(1);
        }

        SetInvincible(_minions.Count > 0);
    }

    private void SummonMinions(int count)
    {
        if (!minionPrefab) return;

        for (int i = 0; i < count; i++)
        {
            int slot = count == minionCount ? i : Random.Range(0, minionCount);
            float angle = 45f + slot * (360f / minionCount);
            Vector3 position = transform.position + (Vector3)(AngleToDirection(angle) * minionSpawnDistance);
            position.z = 0f;

            var minion = Instantiate(minionPrefab, position, Quaternion.identity, transform.parent);

            var minionNpc = minion.GetComponent<BaseNPCBehaviour>();
            if (minionNpc)
            {
                minionNpc.lootBagPrefab = null;
                minionNpc.xpValue = 0;
            }

            _minions.Add(minion);
            ParticleManager.Singleton.SpawnParticles(minion.transform, Color.white, 20);
        }

        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Teleport);
        SetInvincible(_minions.Count > 0);
    }

    private void SetInvincible(bool invincible)
    {
        if (_isInvincible == invincible) return;
        _isInvincible = invincible;
        _npc.invincible = invincible;

        // The status effect is only the visible icon; the bool above blocks the damage
        if (invincible) _npc.ApplyStatusEffect(Constants.StatusEffects.Invincible, 99999f);
        else _npc.RemoveStatusEffect(Constants.StatusEffects.Invincible);
    }

    private void UpdateBombs(Vector2 playerPos)
    {
        if (Time.time < _nextBombTime) return;
        _nextBombTime = Time.time + bombInterval;

        _bombLobber.Lob(playerPos + _playerVelocity * bombLeadSeconds);
        // if (_hasAttackParam) _animator.SetTrigger(AttackParam);
    }

    private void TrackPlayerVelocity(Vector2 playerPos)
    {
        if (_hasLastPlayerPos && Time.deltaTime > 0f)
        {
            var raw = (playerPos - _lastPlayerPos) / Time.deltaTime;
            _playerVelocity = Vector2.Lerp(_playerVelocity, raw, 0.25f);
        }

        _lastPlayerPos = playerPos;
        _hasLastPlayerPos = true;
    }

    private void FireNova()
    {
        int count = Mathf.Max(8, Mathf.CeilToInt(2f * Mathf.PI * novaCoverRadius / Mathf.Max(0.1f, novaMaxGap)));
        float step = 360f / count;
        float offset = Random.Range(0f, step);

        for (int i = 0; i < count; i++)
        {
            FireProjectile(
                AngleToDirection(offset + i * step),
                novaProjectileSprite,
                novaProjectileSpeed,
                novaProjectileScale,
                novaDamage,
                novaParticleColor,
                NoStatusEffects
            );
        }

        ParticleManager.Singleton.SpawnParticles(transform, novaParticleColor, 40);
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.FireSphere);
    }

    private void Shout(string line)
    {
        SpeechBubbles.Say(transform, line);
        PlayerLog.Singleton.AddItem($"\"{line}\" the Lich shouts");
    }

    private void FireProjectile(
        Vector2 direction,
        Sprite sprite,
        float speed,
        float scale,
        float damage,
        Color particleColor,
        List<Constants.StatusEffects> statusEffects
    )
    {
        var position = transform.position;
        position.z = 0f;

        var proj = Instantiate(_projectilePrefab, position, Quaternion.identity);
        var projectile = proj.GetComponent<Projectile>();
        projectile.SilentWallHits = true;
        projectile.Setup(new ProjectileSetupModel(
            direction,
            projectileRotationOffset,
            speed,
            scale,
            damage,
            sprite,
            DamagesPlayer,
            PenetratesEnemies,
            particleColor,
            statusEffects,
            projectileRange,
            null,
            null
        ));
    }

    private static Vector2 AngleToDirection(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void FacePlayer(Vector2 playerPos)
    {
        if (_spriteRenderer) _spriteRenderer.flipX = playerPos.x < transform.position.x;
    }

    private static bool TryGetPlayerPosition(out Vector2 position)
    {
        if (Player.Singleton)
        {
            position = Player.Singleton.transform.position;
            return true;
        }
        position = Vector2.zero;
        return false;
    }

    private void OnDestroy()
    {
        foreach (var minion in _minions)
        {
            if (minion) Destroy(minion);
        }
        _minions.Clear();
    }
}
