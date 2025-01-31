using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour, IDamageable
{
    [SerializeField] private float deathTime;
    [SerializeField] private GameObject hitEffectPrefab;
    private GameObject hitEffectInstance;
    protected float currentHealth;
    protected BasicEnemy basicEnemy;
    private EnemyKnockback enemyKnockback;
    private EnemyAnimationController animationController;
    protected SpriteRenderer spriteRenderer;
    protected bool isDead;

    // 상태이상 관련 변수
    private float damageMultiplier = 1f;

    private EnemyHealthElite enemyHealthElite;
    private EnemyHealthBoss enemyHealthBoss;
    private ResultManager resultManager;
    private float damageModifier = 1f;
    public event System.Action OnDie;

    protected virtual void Awake()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        animationController = GetComponent<EnemyAnimationController>();

        enemyHealthElite = GetComponent<EnemyHealthElite>();
        enemyHealthBoss = GetComponent<EnemyHealthBoss>();
        hitEffectInstance = Instantiate(hitEffectPrefab, transform);
        hitEffectInstance.SetActive(false);
    }

    public virtual void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        if (IsDead()) return;

        float finalDamage = (damage * damageMultiplier) * damageModifier;
        ResultManager.Instance.AddDamage(finalDamage);
        currentHealth -= finalDamage;

        PlayHitEffect();

        if (animationController != null)
        {
            animationController.PlayHitAnimation();
        }

        if (enemyHealthElite != null)
        {
            enemyHealthElite.UpdateHealthBar(currentHealth, basicEnemy.GetMaxHealth());
            enemyHealthElite.HandleDamageVisuals();
        }

        if (enemyHealthBoss != null)
        {
            enemyHealthBoss.UpdateHealthBar(currentHealth, basicEnemy.GetMaxHealth());
            enemyHealthBoss.HandleDamageVisuals();
        }

        if (currentHealth <= 0 && !IsDead())
        {
            Die();
        }
        else if (enemyKnockback != null)
        {
            enemyKnockback.ApplyKnockback(knockbackDirection);
        }

        if (DamageIndicator.Instance != null)
        {
            DamageIndicator.Instance.ShowDamage(transform.position, Mathf.RoundToInt(finalDamage));
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public virtual void Die()
    {
        if (!IsDead())
        {
            isDead = true;

            EnemyStatusEffect statusEffect = GetComponent<EnemyStatusEffect>();
            if (statusEffect != null)
            {
                foreach (EnemyStatusEffect.StatusEffectType type in System.Enum.GetValues(typeof(EnemyStatusEffect.StatusEffectType)))
                {
                    statusEffect.RemoveStatusEffect(type);
                }
            }
        }

        basicEnemy.SpawnExpOrbs();
        OnDie?.Invoke();
        ResultManager.Instance.AddKill();

        FadeOutAndDestroy();
    }

    protected virtual void FadeOutAndDestroy()
    {
        // 모든 자식 오브젝트의 SpriteRenderer 컴포넌트를 가져옴
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in allSprites)
        {
            DOTween.Kill(sprite);
            Color startColor = sprite.color;
            sprite.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
                .SetAutoKill(true);
        }

        // 마지막 스프라이트가 페이드아웃되면 오브젝트를 풀로 반환
        if (allSprites.Length > 0)
        {
            allSprites[0].DOColor(new Color(allSprites[0].color.r, allSprites[0].color.g, allSprites[0].color.b, 0f), deathTime)
                .SetAutoKill(true)
                .OnComplete(() => basicEnemy.ReturnToPool());
        }
    }

    public virtual void Reset()
    {
        currentHealth = basicEnemy.GetMaxHealth();
        isDead = false;

        // 모든 자식 스프라이트 초기화
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in allSprites)
        {
            Color color = sprite.color;
            sprite.color = new Color(color.r, color.g, color.b, 1f);
        }

        // hitEffectInstance 초기화
        if (hitEffectInstance != null)
        {
            hitEffectInstance.SetActive(false);
            SpriteRenderer[] hitEffectSprites = hitEffectInstance.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sprite in hitEffectSprites)
            {
                Color color = sprite.color;
                sprite.color = new Color(color.r, color.g, color.b, 1f);
            }
        }
    }

    // 상태이상 관련 메서드
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    private void PlayHitEffect()
    {
        hitEffectInstance.SetActive(true);
        // 애니메이션 종료 후 자동으로 비활성화되도록 설정
        StartCoroutine(DeactivateEffect());
    }

    private IEnumerator DeactivateEffect()
    {
        yield return new WaitForSeconds(0.4f); // 애니메이션 길이만큼 대기
        hitEffectInstance.SetActive(false);
    }

    public void SetDamageModifier(float modifier)
    {
        damageModifier = modifier;
    }

    public float GetDamageModifier()
    {
        return damageModifier;
    }

    public bool ResetDamageModifier()
    {
        damageModifier = 1f;
        return true;
    }
}