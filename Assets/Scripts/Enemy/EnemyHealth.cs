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

        OnDie?.Invoke();

        FadeOutAndDestroy();
    }

    protected virtual void FadeOutAndDestroy()
    {
        if (spriteRenderer != null)
        {
            DOTween.Kill(spriteRenderer);
            Color startColor = spriteRenderer.color;
            spriteRenderer.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
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