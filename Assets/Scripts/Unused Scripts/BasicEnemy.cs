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

    [Header("Attack Settings")]
    [SerializeField] private GameObject attackAreaPrefab;    // 공격 범위 시각화를 위한 프리팹

    public EnemyStats.EnemyType GetEnemyType() => stats.enemyType;
    public float GetMoveSpeed() => stats.MoveSpeed;
    public float GetAttackRange() => stats.AttackRange;
    public float GetRetreatSpeed() => stats.RetreatSpeed;
    public float GetMaxHealth() => stats.MaxHealth;
    public float GetAttackDelay() => stats.attackDelay;
    public float GetAttackCooldown() => stats.attackCooldown;
    public float GetAttackDamage() => stats.attackDamage;
    public bool IsAttacking { get; private set; } = false;
    public bool CanAttack { get; private set; } = true;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    protected bool isDead = false;
    private ExpOrbPool expOrbPool;
    private EnemyKnockback enemyKnockback;
    public event System.Action<GameObject> OnEnemyDeath;
    private Transform playerTransform;
    private GameObject attackAreaInstance;
    private SpriteRenderer attackAreaSpriteRenderer;
    private Vector2 attackDirection;
    private float attackAngle;
    private Rigidbody2D rb;
    private RigidbodyConstraints2D originalConstraints;
    private Coroutine attackRoutine;
    private EnemyAnimationController animationController;
    [SerializeField] private Color endcolor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        rb = GetComponent<Rigidbody2D>();
        DOTween.SetTweensCapacity(1000, 500);
    }

    private void Start()
    {
        ResetEnemy();
        expOrbPool = FindAnyObjectByType<ExpOrbPool>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        animationController = GetComponent<EnemyAnimationController>();

        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (rb != null)
        {
            rb.constraints = rb.constraints | RigidbodyConstraints2D.FreezeRotation;
            originalConstraints = rb.constraints;
        }
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

        if (this is EliteEnemy)
        {
            MonsterHealthUI healthUI = GetComponent<MonsterHealthUI>();
            healthUI.UpdateHealthBar(currentHealth, stats.MaxHealth);
        }

        if (this is EnemyBoss)
        {
            BossHealthUI bossHealthUI = GetComponent<BossHealthUI>();
            bossHealthUI.UpdateHealthBar(currentHealth, stats.MaxHealth);
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
            StopAttack();
            FadeOutAndDestroy();
            OnEnemyDeath?.Invoke(gameObject);

            if (gameObject.CompareTag("Elite"))
            {
                MonsterHealthUI healthUI = GetComponent<MonsterHealthUI>();
                healthUI.DestroyHealthBar();
            }

            if (gameObject.CompareTag("Boss"))
            {
                BossHealthUI bossHealthUI = GetComponent<BossHealthUI>();
                bossHealthUI.DestroyHealthBar();
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
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            spriteRenderer.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
                .SetAutoKill(false)
                .OnComplete(() =>
                {
                    if (gameObject != null && gameObject.activeInHierarchy)
                    {
                        ReturnToPool();
                    }
                });
        }
    }

    private void ReturnToPool()
    {
        // 오브젝트를 풀로 반환하는 로직
        gameObject.SetActive(false);
        OnEnemyDeath?.Invoke(gameObject);
    }

    private void OnDisable()
    {
        StopAttack();
        DOTween.Kill(spriteRenderer);
    }

    public void CheckAttack()
    {
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= stats.AttackRange && CanAttack)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        CanAttack = false;

        // 공격 시작 시 위치 고정
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        // 공격 시작 시 플레이어 방향 저장
        attackDirection = (playerTransform.position - transform.position).normalized;
        attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        // 공격 범위 표시
        ShowAttackArea();

        // 공격 딜레이 기다림
        yield return new WaitForSeconds(stats.attackDelay);
        animationController.PlayAttackAnimation();

        // 공격 실행
        ExecuteAttack();

        // 공격 쿨다운 대기
        yield return new WaitForSeconds(stats.attackCooldown);

        // 공격 종료 시 위치 고정 해제
        if (rb != null)
        {
            rb.constraints = originalConstraints;
        }

        IsAttacking = false;
        CanAttack = true;
    }

    private void ShowAttackArea()
{
    if (attackAreaPrefab != null)
    {
        if (attackAreaInstance == null)
        {
            attackAreaInstance = Instantiate(attackAreaPrefab, transform.position, Quaternion.identity, transform);
            attackAreaSpriteRenderer = attackAreaInstance.GetComponent<SpriteRenderer>();
        }
        else
        {
            attackAreaInstance.SetActive(true);
        }

        attackAngle -= 90f;
        attackAreaInstance.transform.rotation = Quaternion.Euler(0, 0, attackAngle);

        float range = stats.AttackRange;
        attackAreaInstance.transform.localScale = new Vector3(range, range, 1);

        attackAreaSpriteRenderer.color = Color.red;
        attackAreaSpriteRenderer.DOColor(endcolor, stats.attackDelay)
            .OnComplete(() => {
                attackAreaInstance.SetActive(false);
            });
    }
}

    private void ExecuteAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.AttackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector2 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector2.Angle(attackDirection, directionToTarget);

                if (angleToTarget <= 30f)
                {
                    IDamageable playerDamageable = hit.GetComponent<IDamageable>();
                    if (playerDamageable != null)
                    {
                        playerDamageable.TakeDamage(stats.attackDamage, Vector2.zero);
                    }
                }
            }
        }
    }

    private void StopAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (rb != null)
        {
            rb.constraints = originalConstraints;
        }
        IsAttacking = false;
        CanAttack = true;
    }

    public void ResetEnemy()
    {
        StopAttack();
        currentHealth = stats.MaxHealth;
        isDead = false;
        CanAttack = true;
        IsAttacking = false;

        if (enemyKnockback != null)
        {
            enemyKnockback.ResetColor();
        }
    }
}