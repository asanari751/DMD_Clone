using DG.Tweening;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour, IDamageable
{
    [SerializeField] private float deathTime;
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
    public event System.Action OnDie;

    protected virtual void Awake()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        animationController = GetComponent<EnemyAnimationController>();

        enemyHealthElite = GetComponent<EnemyHealthElite>();
        enemyHealthBoss = GetComponent<EnemyHealthBoss>();
    }

    public virtual void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        if (IsDead())
        {
            return; // 사망시 중단
        }

        float finalDamage = damage * damageMultiplier;
        currentHealth -= finalDamage;

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
        }

        basicEnemy.SpawnExpOrbs();
        OnDie?.Invoke();

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
    }

    // 상태이상 관련 메서드
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }
}