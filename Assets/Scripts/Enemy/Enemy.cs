using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class BasicEnemy : MonoBehaviour
{
    [SerializeField] private float experienceDrop;
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private int expOrbCount;
    [SerializeField] private EnemyStats stats;
    private AudioManager audioManager;

    [Header("Attack Settings")]
    [SerializeField] private GameObject attackAreaPrefab;    // 공격 범위 시각화를 위한 프리팹

    public EnemyStats.EnemyType GetEnemyType() => stats.enemyType;
    public float GetBasedSpeed() => stats.MoveSpeed;
    public float GetAttackRange() => stats.AttackRange;
    public float GetRetreatSpeed() => stats.RetreatSpeed;
    public float GetMaxHealth() => stats.MaxHealth;
    public float GetAttackDelay() => stats.attackDelay;
    public float GetAttackCooldown() => stats.attackCooldown;
    public float GetAttackDamage() => stats.attackDamage;
    public bool IsAttacking { get; private set; } = false;
    public bool CanAttack { get; private set; } = true;

    private SpriteRenderer spriteRenderer;
    private ExpOrbPool expOrbPool;
    private EnemyKnockback enemyKnockback;
    public event System.Action<GameObject> OnEnemyDeath;
    protected Transform playerTransform;
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
    private float attackDelayAfterKnockback = 0.5f;
    private float currentAttackDelayTimer = 0f;
    private bool isWaitingToAttack = false;
    private EnemyStatusEffect statusEffect;
    private bool canMove = true;
    private float currentMoveSpeed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        statusEffect = GetComponent<EnemyStatusEffect>();
        audioManager = FindAnyObjectByType<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        currentMoveSpeed = stats.MoveSpeed;
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

    // private void Update()
    // {
    //     if (spriteRenderer != null)
    //     {
    //         spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    //     }
    // }

    private void OnDestroy()
    {
        DOTween.Kill(spriteRenderer);
        if (attackAreaSpriteRenderer != null)
        {
            DOTween.Kill(attackAreaSpriteRenderer);
        }
    }

    public bool IsKnockedBack()
    {
        return enemyKnockback != null && enemyKnockback.IsKnockedBack();
    }

    public void SpawnExpOrbs()
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

    public void ReturnToPool()
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
    public virtual void CheckAttack()
    {
        if (GetComponent<EnemyHealthController>().IsDead() || playerTransform == null) return;

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
            attackAreaInstance.transform.localScale = new Vector3(range / 3, range / 3, 1);

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
        if (stats.enemyType == EnemyStats.EnemyType.Melee)
        {
            audioManager.PlaySFX("S21");
        }
        else if (stats.enemyType == EnemyStats.EnemyType.Arrow)
        {
            audioManager.PlaySFX("S22");
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.AttackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector2 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector2.Angle(attackDirection, directionToTarget);

                if (angleToTarget <= stats.attackAngleRange)
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

    public void StopAttack()
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
        GetComponent<EnemyHealthController>().Reset();
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

    // 상태이상

    public float GetMoveSpeed() => currentMoveSpeed;

    public void SetMoveSpeed(float speed)
    {
        currentMoveSpeed = speed;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public bool CanMove()
    {
        return canMove && !IsKnockedBack() && !IsAttacking;
    }

    private void OnDrawGizmosSelected()
    {
        if (stats != null)
        {
            // 공격 범위 시각화 (흰색 와이어프레임)
            Gizmos.color = new Color(1, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, stats.AttackRange);

            // 공격 범위 채우기 (반투명 흰색)
            Gizmos.color = new Color(1, 1, 1, 0.1f);
            Gizmos.DrawSphere(transform.position, stats.AttackRange);

            // 공격 방향 시각화 (빨간색 선)
            if (IsAttacking && playerTransform != null)
            {
                Gizmos.color = Color.red;
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                Gizmos.DrawLine(transform.position, transform.position + direction * stats.AttackRange);
            }
        }
    }
}