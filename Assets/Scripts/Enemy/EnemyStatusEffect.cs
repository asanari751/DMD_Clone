using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class EnemyStatusEffect : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Debug Options")]
    [SerializeField] private StatusEffectType debugStatusEffect;
    [SerializeField] private float debugDuration = 3f;
    [SerializeField] private bool applyDebugEffect;

    private void OnValidate()
    {
        if (applyDebugEffect)
        {
            if (statusEffectVisual != null && statusEffectAnimator != null)
            {
                ApplyStatusEffect(debugStatusEffect, debugDuration);
            }
            applyDebugEffect = false;
        }
    }
#endif

    [Header("Status Effect Values")]
    [SerializeField] private float slowDebuff = 0.75f;
    [SerializeField] private float weaknessDebuff = 1.3f;
    [SerializeField] public float poisonDebuff = 3f;
    [SerializeField] private float bleedDebuff = 0.1f;

    [Header("Animation States")]
    [SerializeField] private GameObject statusEffectVisual;
    [SerializeField] private string poisonStateName = "Poison_Effect";
    [SerializeField] private string bleedStateName = "Bleed_Effect";
    [SerializeField] private string stunStateName = "Stun_Effect";
    [SerializeField] private string fearStateName = "Fear_Effect";
    [SerializeField] private string slowStateName = "Slow_Effect";
    [SerializeField] private string weaknessStateName = "Weakness_Effect";

    public enum StatusEffectType
    {
        None,
        Fear,
        Bleed,
        Slow,
        Weakness,
        Poison,
        Stun
    }

    private Dictionary<StatusEffectType, Coroutine> activeStatusEffects = new Dictionary<StatusEffectType, Coroutine>();
    private SpriteRenderer spriteRenderer;
    private BasicEnemy basicEnemy;
    private EnemyHealthController enemyHealthController;
    private bool isFeared = false;
    private Animator statusEffectAnimator;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        basicEnemy = GetComponent<BasicEnemy>();
        enemyHealthController = GetComponent<EnemyHealthController>();
        if (statusEffectVisual != null)
        {
            statusEffectAnimator = statusEffectVisual.GetComponent<Animator>();
        }
    }

    public void ApplyStatusEffect(StatusEffectType type, float duration)
    {
        if (activeStatusEffects.ContainsKey(type))
        {
            StopCoroutine(activeStatusEffects[type]);
        }

        switch (type)
        {
            case StatusEffectType.Fear:
                activeStatusEffects[type] = StartCoroutine(FearEffect(duration));
                break;
            case StatusEffectType.Bleed:
                activeStatusEffects[type] = StartCoroutine(BleedEffect(duration));
                break;
            case StatusEffectType.Slow:
                activeStatusEffects[type] = StartCoroutine(SlowEffect(duration));
                break;
            case StatusEffectType.Weakness:
                activeStatusEffects[type] = StartCoroutine(WeaknessEffect(duration));
                break;
            case StatusEffectType.Poison:
                activeStatusEffects[type] = StartCoroutine(PoisonEffect(duration));
                break;
            case StatusEffectType.Stun:
                activeStatusEffects[type] = StartCoroutine(StunEffect(duration));
                break;
        }
    }

    public void RemoveStatusEffect(StatusEffectType type)
    {
        if (activeStatusEffects.ContainsKey(type))
        {
            StopCoroutine(activeStatusEffects[type]);
            activeStatusEffects.Remove(type);

            if (type == StatusEffectType.Fear)
            {
                isFeared = false;
            }
        }
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeStatusEffects.ContainsKey(type);
    }

    // Effects

    private IEnumerator FearEffect(float duration)
    {
        statusEffectAnimator.Play(fearStateName, 0, 0f);
        isFeared = true;
        basicEnemy.StopAttack();

        Sequence fearSequence = DOTween.Sequence();
        fearSequence.Append(spriteRenderer.DOColor(Color.grey, 0.2f))
                    .Append(spriteRenderer.DOColor(Color.white, 0.2f))
                    .SetLoops(-1);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fearSequence.Kill();
        spriteRenderer.color = Color.white;

        isFeared = false;
        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Fear);
    }

    private IEnumerator BleedEffect(float duration)
    {
        statusEffectAnimator.Play(bleedStateName, 0, 0f);
        float elapsedTime = 0f;
        float tickInterval = 1f;
        float nextTickTime = tickInterval;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= nextTickTime)
            {
                ApplyBleedDamage();
                nextTickTime += tickInterval;
            }

            yield return null;
        }

        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Bleed);
    }

    private IEnumerator SlowEffect(float duration)
    {
        statusEffectAnimator.Play(slowStateName, 0, 0f);
        float originalSpeed = basicEnemy.GetMoveSpeed();
        basicEnemy.SetMoveSpeed(originalSpeed * slowDebuff);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        basicEnemy.SetMoveSpeed(originalSpeed); // 원래 속도로 복구
        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Slow);
    }

    private IEnumerator WeaknessEffect(float duration)
    {
        statusEffectAnimator.Play(weaknessStateName, 0, 0f);
        enemyHealthController.SetDamageMultiplier(weaknessDebuff); // 30% 데미지 증가

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyHealthController.SetDamageMultiplier(1f); // 데미지 배율 원래대로
        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Weakness);
    }

    private IEnumerator PoisonEffect(float duration)
    {
        statusEffectAnimator.Play(poisonStateName, 0, 0f);
        float elapsedTime = 0f;
        float tickInterval = 1f;
        float nextTickTime = tickInterval;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= nextTickTime)
            {
                enemyHealthController.TakeDamage(poisonDebuff, Vector2.zero);
                nextTickTime += tickInterval;
            }

            yield return null;
        }

        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Poison);
    }

    private IEnumerator StunEffect(float duration)
    {
        statusEffectAnimator.Play(stunStateName, 0, 0f);
        basicEnemy.SetCanMove(false);

        Color originalColor = spriteRenderer.color;
        spriteRenderer.DOColor(Color.yellow, 0.2f).OnComplete(() =>
        {
            spriteRenderer.DOColor(originalColor, 0.2f);
        });

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        basicEnemy.SetCanMove(true);
        statusEffectAnimator.Play("None", 0, 0f);
        activeStatusEffects.Remove(StatusEffectType.Stun);
    }

    private void ApplyBleedDamage()
    {
        float bleedDamage = basicEnemy.GetMaxHealth() * bleedDebuff;
        enemyHealthController.TakeDamage(bleedDamage, Vector2.zero);
    }

    public bool IsFeared()
    {
        return isFeared;
    }

    private void OnDisable()
    {
        foreach (var effect in activeStatusEffects)
        {
            StopCoroutine(effect.Value);
        }
        activeStatusEffects.Clear();
        isFeared = false;
    }

    public void ClearAllStatusEffects()
    {
        foreach (var effect in activeStatusEffects)
        {
            StopCoroutine(effect.Value);
        }
        activeStatusEffects.Clear();
        isFeared = false;

        if (statusEffectAnimator != null)
        {
            statusEffectAnimator.Play("None", 0, 0f);
        }
    }

    private string GetAnimationState(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Poison => poisonStateName,
            StatusEffectType.Bleed => bleedStateName,
            StatusEffectType.Stun => stunStateName,
            StatusEffectType.Fear => fearStateName,
            StatusEffectType.Slow => slowStateName,
            StatusEffectType.Weakness => weaknessStateName,
            _ => ""
        };
    }
}
