using UnityEngine;
using System.Collections;
using DG.Tweening;

public class BasicEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float deathTime;
    [SerializeField] private float experienceDrop;
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private int expOrbCount;
    [SerializeField] private EnemyStats stats;

    public EnemyStats.EnemyType GetEnemyType() => stats.enemyType;
    public float GetMoveSpeed() => stats.MoveSpeed;
    public float GetAttackRange() => stats.AttackRange;
    public float GetRetreatSpeed() => stats.RetreatSpeed;
    public float GetMaxHealth() => stats.MaxHealth;


    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    protected bool isDead = false;
    private ExpOrbPool expOrbPool;
    private Sequence knockbackSequence;
    private EnemyKnockback enemyKnockback;
    public event System.Action<GameObject> OnEnemyDeath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        DOTween.SetTweensCapacity(1000, 500);
    }

    private void Start()
    {
        ResetEnemy();
        expOrbPool = FindAnyObjectByType<ExpOrbPool>();

    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
        else if (enemyKnockback != null)
        {
            enemyKnockback.ApplyKnockback(knockbackDirection);
        }

        if (DamageIndicator.Instance != null)
        {
            DamageIndicator.Instance.ShowDamage(transform.position, Mathf.RoundToInt(damage));
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsKnockedBack()
    {
        return enemyKnockback != null && enemyKnockback.IsKnockedBack();
    }

    public virtual void Die()
    {
        if (!isDead)
        {
            isDead = true;
            FadeOutAndDestroy();
            OnEnemyDeath?.Invoke(gameObject);

            if (gameObject.CompareTag("Elite"))
            {
                MonsterHealthUI healthUI = GetComponent<MonsterHealthUI>();
                healthUI.DestroyHealthBar();
            }

            SpawnExpOrbs();
        }
    }

    private void SpawnExpOrbs()
    {
        for (int i = 0; i < expOrbCount; i++)
        {
            GameObject expOrb = expOrbPool.GetExpOrb();
            if (expOrb != null)
            {
                expOrb.transform.position = transform.position;
                expOrb.SetActive(true);
                ExpOrb orbScript = expOrb.GetComponent<ExpOrb>();
                if (orbScript != null)
                {
                    orbScript.SetExperience(experienceDrop / expOrbCount);
                }
            }
        }
    }

    private void FadeOutAndDestroy()
    {
        Color startColor = spriteRenderer.color;

        spriteRenderer.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    private void OnDisable()
    {
        DOTween.Kill(spriteRenderer);
    }

    public void ResetEnemy()
    {
        currentHealth = stats.MaxHealth;
        isDead = false;

        if (enemyKnockback != null)
        {
            enemyKnockback.ResetColor();
        }
    }
}