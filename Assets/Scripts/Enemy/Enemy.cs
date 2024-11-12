using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class BasicEnemy : MonoBehaviour, IDamageable
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
    [SerializeField] private Color startColor;
    [SerializeField] private Color endcolor;
    private Dictionary<StatusEffectType, Coroutine> activeStatusEffects = new Dictionary<StatusEffectType, Coroutine>();
    private bool isFeared = false;
    private float attackDelayAfterKnockback = 0.5f;
    private float currentAttackDelayTimer = 0f;
    private bool isWaitingToAttack = false;

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

    public interface IStatusEffectable
    {
        void ApplyStatusEffect(StatusEffectType type, float duration);
        void RemoveStatusEffect(StatusEffectType type);
        bool HasStatusEffect(StatusEffectType type);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        rb = GetComponent<Rigidbody2D>();
        DOTween.SetTweensCapacity(1000, 500);
    }

    protected void Start()
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

    private void Update()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        if (isFeared)
        {
            return; // 공포 상태일 때는 일반 업데이트 로직 실행하지 않음
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(spriteRenderer);
        if (attackAreaSpriteRenderer != null)
        {
            DOTween.Kill(attackAreaSpriteRenderer);
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        if (animationController != null)
        {
            animationController.PlayHitAnimation();
        }

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
        if (expOrbPool == null) return; // 호출 실패 시 처리

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
            // 기존 Tween 제거
            DOTween.Kill(spriteRenderer);

            Color startColor = spriteRenderer.color;
            var tween = spriteRenderer.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
                .SetAutoKill(true)
                .OnComplete(() =>
                {
                    if (this != null && gameObject != null && gameObject.activeInHierarchy)
                    {
                        ReturnToPool();
                    }
                });
        }
    }

    private void ReturnToPool()
    {
        if (attackAreaInstance != null)
        {
            Destroy(attackAreaInstance);
            attackAreaInstance = null;
        }
        gameObject.SetActive(false);
        OnEnemyDeath?.Invoke(gameObject);
    }

    private void OnDisable()
    {
        StopAttack();
        if (spriteRenderer != null)
        {
            DOTween.Kill(spriteRenderer);
        }
    }
    public void CheckAttack()
    {
        if (isDead || playerTransform == null || isFeared) return;

        PlayerHealthUI playerHealth = playerTransform.GetComponent<PlayerHealthUI>();
        if (playerHealth != null && playerHealth.IsDead()) return;

        if (IsKnockedBack())
        {
            isWaitingToAttack = true;
            currentAttackDelayTimer = 0f;
            return;
        }

        if (isWaitingToAttack)
        {
            currentAttackDelayTimer += Time.deltaTime;
            if (currentAttackDelayTimer >= attackDelayAfterKnockback)
            {
                isWaitingToAttack = false;
            }
            return;
        }

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

            attackAreaSpriteRenderer.color = startColor; // 시작 색을 startColor로 변경
            attackAreaSpriteRenderer.DOColor(endcolor, stats.attackDelay)
                .OnComplete(() =>
                {
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
                    PlayerHealthUI playerHealth = hit.GetComponent<PlayerHealthUI>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(stats.attackDamage);
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
        if (attackAreaInstance != null)
        {
            Destroy(attackAreaInstance);
            attackAreaInstance = null;
        }
        currentHealth = stats.MaxHealth;
        isDead = false;
        CanAttack = true;
        IsAttacking = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = originalConstraints;
        }

        if (enemyKnockback != null)
        {
            enemyKnockback.ResetColor();
        }
    }

    // ========= 상태이상 ( 별도 컴포넌트로 분리후 제거 )

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

    private IEnumerator FearEffect(float duration)
    {
        isFeared = true;
        StopAttack();

        // 깜빡임 효과 시작
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

        // 깜빡임 효과 종료 및 원래 색상으로 복구
        fearSequence.Kill();
        spriteRenderer.color = Color.white;

        isFeared = false;
        activeStatusEffects.Remove(StatusEffectType.Fear);
    }

    public bool IsFeared()
    {
        return isFeared;
    }

    private void ApplyBleedDamage()
    {
        float bleedDamage = GetMaxHealth() * 0.1f;
        currentHealth -= bleedDamage;

        // 체력바 업데이트
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

        if (DamageIndicator.Instance != null)
        {
            DamageIndicator.Instance.ShowDamage(transform.position, Mathf.RoundToInt(bleedDamage));
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
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
}