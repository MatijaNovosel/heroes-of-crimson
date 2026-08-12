using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public Image healthbarImage;
    public Image healthbarImageLight;
    public Image damageBarImage;

    public TMP_Text healthBarText;
    public TMP_Text regenText;

    [Header("Damage Trail Animation")]
    [Min(0f)]
    public float damageHoldTime = 0.12f;

    [Min(0f)]
    public float damageShrinkTime = 0.25f;

    public AnimationCurve damageShrinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private BaseNPCBehaviour _baseNPC;

    private Coroutine _damageRoutine;

    private float _previousHp;
    private float _previousFill;
    private bool _initialized;

    private void Awake()
    {
        if (player != null)
        {
            _baseNPC = player.GetComponent<BaseNPCBehaviour>();
        }
    }

    private void OnEnable()
    {
        _initialized = false;
    }

    private void Start()
    {
        RefreshHealthBar(immediate: true);
    }

    private void Update()
    {
        RefreshHealthBar(immediate: false);
    }

    private void OnDisable()
    {
        StopDamageRoutine();
    }

    public void UpdateRegenText(float regenPerSecond)
    {
        regenText.text = $"+{regenPerSecond:F2}";
    }

    private void RefreshHealthBar(bool immediate)
    {
        if (_baseNPC == null)
        {
            ShowDeadState();
            return;
        }

        float hp = Mathf.Max(0f, _baseNPC.hp);
        float maxHp = Mathf.Max(1f, _baseNPC.maxHp);
        float fill = Mathf.Clamp01(hp / maxHp);

        healthbarImage.fillAmount = fill;
        healthbarImageLight.fillAmount = fill;

        healthBarText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}";

        if (immediate || !_initialized)
        {
            StopDamageRoutine();

            damageBarImage.fillAmount = fill;

            _previousHp = hp;
            _previousFill = fill;
            _initialized = true;
            return;
        }

        const float hpComparisonEpsilon = 0.0001f;

        bool receivedDamage = hp < _previousHp - hpComparisonEpsilon;

        if (receivedDamage)
        {
            StartDamageTrail(
                fillBeforeDamage: _previousFill,
                targetFill: fill
            );
        }
        else if (_damageRoutine == null)
        {
            damageBarImage.fillAmount = fill;
        }

        _previousHp = hp;
        _previousFill = fill;
    }

    private void StartDamageTrail(float fillBeforeDamage, float targetFill)
    {
        StopDamageRoutine();

        damageBarImage.fillAmount = Mathf.Max(
            damageBarImage.fillAmount,
            fillBeforeDamage
        );

        _damageRoutine = StartCoroutine(
            AnimateDamageTrail(targetFill)
        );
    }

    private IEnumerator AnimateDamageTrail(float targetFill)
    {
        if (damageHoldTime > 0f)
        {
            yield return new WaitForSecondsRealtime(damageHoldTime);
        }

        float startFill = damageBarImage.fillAmount;

        if (damageShrinkTime <= 0f)
        {
            damageBarImage.fillAmount = healthbarImage.fillAmount;
            _damageRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < damageShrinkTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / damageShrinkTime);
            float easedTime = Mathf.Clamp01(damageShrinkCurve.Evaluate(normalizedTime));

            float animatedFill = Mathf.Lerp(
                startFill,
                targetFill,
                easedTime
            );

            damageBarImage.fillAmount = Mathf.Max(
                animatedFill,
                healthbarImage.fillAmount
            );

            yield return null;
        }

        damageBarImage.fillAmount = healthbarImage.fillAmount;
        _damageRoutine = null;
    }

    private void StopDamageRoutine()
    {
        if (_damageRoutine == null)
        {
            return;
        }

        StopCoroutine(_damageRoutine);
        _damageRoutine = null;
    }

    private void ShowDeadState()
    {
        StopDamageRoutine();

        healthbarImage.fillAmount = 0f;
        healthbarImageLight.fillAmount = 0f;
        damageBarImage.fillAmount = 0f;

        healthBarText.text = "Dead";
        regenText.text = "";

        _previousHp = 0f;
        _previousFill = 0f;
        _initialized = false;
    }
}