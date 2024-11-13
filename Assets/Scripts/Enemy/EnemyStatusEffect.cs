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
            ApplyStatusEffect(debugStatusEffect, debugDuration);
            applyDebugEffect = false;
        }
    }
#endif

    [Header("Status Effect Values")]
    [SerializeField] private float slowDebuff = 0.75f;
    [SerializeField] private float weaknessDebuff = 1.3f;
    [SerializeField] private float poisonDebuff = 3f;
    [SerializeField] private float bleedDebuff = 0.1f;

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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        basicEnemy = GetComponent<BasicEnemy>();
        enemyHealthController = GetComponent<EnemyHealthController>();
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
        activeStatusEffects.Remove(StatusEffectType.Fear);
    }

    private IEnumerator BleedEffect(float duration)
    {
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

        activeStatusEffects.Remove(StatusEffectType.Bleed);
    }

    private IEnumerator SlowEffect(float duration)
    {
        float originalSpeed = basicEnemy.GetMoveSpeed();
        basicEnemy.SetMoveSpeed(originalSpeed * slowDebuff);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        basicEnemy.SetMoveSpeed(originalSpeed); // 원래 속도로 복구
        activeStatusEffects.Remove(StatusEffectType.Slow);
    }

    private IEnumerator WeaknessEffect(float duration)
    {
        enemyHealthController.SetDamageMultiplier(weaknessDebuff); // 30% 데미지 증가

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyHealthController.SetDamageMultiplier(1f); // 데미지 배율 원래대로
        activeStatusEffects.Remove(StatusEffectType.Weakness);
    }

    private IEnumerator PoisonEffect(float duration)
    {
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

        activeStatusEffects.Remove(StatusEffectType.Poison);
    }

    private IEnumerator StunEffect(float duration)
    {
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
}
