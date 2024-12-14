using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Linq;

public class PlayerStateManager : MonoBehaviour
{
    public enum AttackType
    {
        Claw,
        Sword,
        Arrow
    }

    [System.Serializable]
    public class MeleeAttackSet
    {
        public GameObject[] attackPrefabs;
        public string[] animationNames;
        public int ComboCount => attackPrefabs.Length;
    }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private Transform firePosition;
    private Camera mainCamera;

    [Header("Default")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] public AttackType currentAttackType;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float attackAngle;
    [SerializeField] private float meeleAttackRange;
    [SerializeField] private float atkDelay;
    [SerializeField] private int penetrateCount;
    [SerializeField] private float projectileSpeed;

    [Header("Projectile Trail")] // Unused
    [SerializeField] private TrailRenderer projectileTrailPrefab;
    [SerializeField] private ProjectileTrailSettings projectileTrailSettings;

    [Header("Melee Attack Sets")]
    [SerializeField] private MeleeAttackSet clawAttackSet;
    [SerializeField] private MeleeAttackSet swordAttackSet;
    [SerializeField] private float comboResetTime = 2f;
    [SerializeField] private Vector2 attackEffectOffset = Vector2.zero;

    // 런타임 상태 변수
    private bool isAutoAttack = false;
    private Tween autoAttackTween;
    private float lastAttackTime = 0f;
    private float lastRangedAttackTime = 0f;
    private int currentComboCount = 0;
    private float lastComboTime = 0f;
    private GameObject currentAttackEffect;

    // 런타임
    public event Action<CharacterInfo.PlayerStats> OnStatsUpdated;

    private void Awake()
    {
        attackAction.action.performed += _ => ToggleAutoAttack();
        mainCamera = Camera.main;

        if (projectilePool == null)
        {
            projectilePool = FindAnyObjectByType<ProjectilePool>();
        }

        UpdateStats();
    }

    private void Start()
    {
        UpdateStats();
    }

    private void OnEnable() => attackAction.action.Enable();
    private void OnDisable() => attackAction.action.Disable();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformManualAttack();
        }

        // 현재 공격 타입에 따라 적절한 lastAttackTime 선택
        float currentCooldown = currentAttackType == AttackType.Arrow ?
            Time.time - lastRangedAttackTime :
            Time.time - lastAttackTime;

        // UpdateSkillCooldown에서 UpdateBasicAttackCooldown으로 변경
        SkillSelector.Instance?.UpdateBasicAttackCooldown(currentCooldown, atkDelay);
    }

    private void ToggleAutoAttack()
    {
        isAutoAttack = !isAutoAttack;
        SkillSelector.Instance?.ToggleAutoAttackIndicator(isAutoAttack);

        if (isAutoAttack)
        {
            switch (currentAttackType)
            {
                case AttackType.Sword:
                    audioManager.PlaySFX("S2");
                    break;
                case AttackType.Claw:
                    audioManager.PlaySFX("S3");
                    break;
                case AttackType.Arrow:
                    audioManager.PlaySFX("S4");
                    break;
            }

            if (autoAttackTween != null)
            {
                autoAttackTween.Kill();
            }
            float attackInterval = atkDelay;
            autoAttackTween = DOVirtual.DelayedCall(attackInterval, PerformAutoAttack, false).SetLoops(-1);
        }
        else
        {
            if (autoAttackTween != null)
            {
                autoAttackTween.Kill();
                autoAttackTween = null;
            }
        }
    }

    private void PerformAutoAttack()
    {
        if (PauseController.Paused == true) return;
        GameObject closestEnemy = FindClosestEnemy();

        if (closestEnemy != null)
        {
            Vector2 targetPosition = closestEnemy.transform.position;
            Action<Vector2> attackAction = currentAttackType == AttackType.Arrow ? PerformRangedAttack : PerformAttack;
            attackAction(targetPosition);
        }
        return;
    }

    private void PerformManualAttack()
    {
        if (PauseController.Paused == true) return;

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mousePosition - (Vector2)transform.position).normalized;
        float distanceToMouse = Vector2.Distance(transform.position, mousePosition);

        Vector2 attackPosition;
        if (distanceToMouse <= detectionRadius)
        {
            attackPosition = mousePosition;
        }
        else
        {
            attackPosition = (Vector2)transform.position + (direction * meeleAttackRange);
        }

        Action<Vector2> attackAction = currentAttackType == AttackType.Arrow ? PerformRangedAttack : PerformAttack;
        attackAction(attackPosition);
    }

    private void PerformRangedAttack(Vector2 targetPosition)
    {
        if (Time.time - lastRangedAttackTime < atkDelay)
        {
            return;
        }

        Vector2 direction = (targetPosition - (Vector2)firePosition.position).normalized;
        audioManager.PlaySFX("S4");

        GameObject projectileObj = projectilePool.GetProjectile();
        projectileObj.transform.position = firePosition.position;
        projectileObj.transform.right = direction;

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        TrailRenderer trail = projectileObj.GetComponent<TrailRenderer>();

        trail.time = projectileTrailSettings.trailDuration;
        trail.startWidth = projectileTrailSettings.trailStartWidth;
        trail.endWidth = projectileTrailSettings.trailEndWidth;
        trail.startColor = projectileTrailSettings.trailStartColor;
        trail.endColor = projectileTrailSettings.trailEndColor;
        trail.enabled = true;

        projectile.Initialize(projectileSpeed, playerStats.defaultDamage, penetrateCount, projectilePool);

        lastRangedAttackTime = Time.time;
    }

    private void PerformAttack(Vector2 targetPosition)
    {
        if (Time.time - lastAttackTime < atkDelay)
        {
            return;
        }

        switch (currentAttackType)
        {
            case AttackType.Sword:
                audioManager.PlaySFX("S2");
                break;
            case AttackType.Claw:
                audioManager.PlaySFX("S3");
                break;
        }

        AttackInSemicircle(targetPosition);
        lastAttackTime = Time.time;
    }

    private GameObject FindClosestEnemy() =>
        Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayers)
            .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
            .FirstOrDefault()?.gameObject;

    private void AttackInSemicircle(Vector2 targetPosition)
    {
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, meeleAttackRange, targetLayers))
        {
            if (IsInAttackAngle(collider.transform.position, angleToTarget))
            {
                EnemyHealthController enemyHealth = collider.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
                    enemyHealth.TakeDamage(playerStats.defaultDamage, -knockbackDirection);
                }
            }
        }
        ShowAttackEffect(targetPosition);
    }

    private bool IsInAttackAngle(Vector3 position, float targetAngle)
    {
        Vector2 directionToCollider = (position - transform.position).normalized;
        float angleToCollider = Mathf.Atan2(directionToCollider.y, directionToCollider.x) * Mathf.Rad2Deg;
        return Mathf.Abs(Mathf.DeltaAngle(targetAngle, angleToCollider)) <= attackAngle / 2;
    }

    private void ShowAttackEffect(Vector2 targetPosition)
    {
        if (Time.time - lastComboTime > comboResetTime)
        {
            currentComboCount = 0;
        }

        if (currentAttackEffect != null)
        {
            Destroy(currentAttackEffect);
        }

        MeleeAttackSet currentAttackSet = currentAttackType == AttackType.Claw ? clawAttackSet : swordAttackSet;
        Vector2 spawnPosition = targetPosition;
        Quaternion rotation = Quaternion.identity;

        if (currentAttackType == AttackType.Sword)
        {
            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Sword 타입일 때만 offset 적용
            Vector2 adjustedOffset = CalculateDirectionalOffset(direction);
            spawnPosition = (Vector2)transform.position + adjustedOffset;

            rotation = Quaternion.Euler(0, 0, angle);
        }

        currentAttackEffect = Instantiate(currentAttackSet.attackPrefabs[currentComboCount], spawnPosition, rotation);

        Animator animator = currentAttackEffect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(currentAttackSet.animationNames[currentComboCount]);
            Destroy(currentAttackEffect, atkDelay);
        }

        currentComboCount = (currentComboCount + 1) % currentAttackSet.ComboCount;
        lastComboTime = Time.time;
    }

    // private void SetAttackEffectTransform(Vector2 targetPosition)
    // {
    //     Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
    //     float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

    //     Vector2 originalSize = attackEffectSprite.sprite.bounds.size;
    //     float scaleX = meeleAttackRange * 2 / originalSize.x;
    //     float scaleY = meeleAttackRange / originalSize.y;

    //     attackEffectSprite.transform.localScale = new Vector3(scaleX, scaleY, 1f);

    //     Vector2 adjustedOffset = CalculateDirectionalOffset(directionToTarget);

    //     Vector3 basePosition = transform.position + (Vector3)adjustedOffset;
    //     attackEffectSprite.transform.position = (Vector2)basePosition + directionToTarget * (meeleAttackRange / 2f);

    //     attackEffectSprite.transform.rotation = Quaternion.Euler(0, 0, angleToTarget - 90);
    // }

    private Vector2 CalculateDirectionalOffset(Vector2 direction)
    {
        Vector2 absDirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        float xOffset = direction.x > 0 ? attackEffectOffset.x : -attackEffectOffset.x;
        float yOffset = direction.y > 0 ? attackEffectOffset.y : -attackEffectOffset.y;

        if (absDirection.x > absDirection.y)
        {
            return new Vector2(xOffset, 0);
        }
        else
        {
            return new Vector2(0, yOffset);
        }
    }

    public void UpdateStats()
    {
        var stats = new CharacterInfo.PlayerStats
        {
            // 1. Attack
            damageMultiplier = playerStats.damageMultiplier,
            criticalDamage = playerStats.criticalDamage,
            criticalChance = playerStats.criticalChance,
            range = playerStats.range,

            // 2. Defense
            maxHealth = playerStats.maxHealth,
            armor = playerStats.armor,
            reduction = playerStats.reduction,
            dodge = playerStats.dodge,

            // 3. Default Attack
            defaultDamage = playerStats.defaultDamage,
            defaultAttackSpeed = playerStats.defaultAttackSpeed,
            defaultRange = playerStats.defaultRange,
            defaultCriticalChance = playerStats.defaultCriticalChance,

            // 4. Utility
            moveSpeed = playerStats.moveSpeed,
            lootRange = playerStats.lootRange,
            dashRange = playerStats.dashRange,
            dashCount = playerStats.dashCount
        };

        OnStatsUpdated?.Invoke(stats);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, meeleAttackRange);

        if (currentAttackType == AttackType.Claw || currentAttackType == AttackType.Sword)
        {
            Gizmos.color = Color.yellow;
            Vector2 mousePos = Camera.main != null ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Vector2.zero;
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Vector3 leftLine = Quaternion.Euler(0, 0, angle - attackAngle / 2) * Vector3.right * meeleAttackRange;
            Vector3 rightLine = Quaternion.Euler(0, 0, angle + attackAngle / 2) * Vector3.right * meeleAttackRange;

            Gizmos.DrawLine(transform.position, transform.position + leftLine);
            Gizmos.DrawLine(transform.position, transform.position + rightLine);
        }

        Collider2D[] detectedEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayers);
        foreach (Collider2D enemy in detectedEnemies)
        {
            Vector2 enemyPos = enemy.transform.position;
            Vector2 direction = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;

            // 삼각형 크기 설정
            float triangleSize = 0.5f;
            Vector2 triangleTop = enemyPos + direction * triangleSize;
            Vector2 triangleLeft = enemyPos + Vector2.Perpendicular(direction) * triangleSize * 0.5f;
            Vector2 triangleRight = enemyPos - Vector2.Perpendicular(direction) * triangleSize * 0.5f;

            // 삼각형 그리기 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(triangleLeft, triangleTop);
            Gizmos.DrawLine(triangleTop, triangleRight);
            Gizmos.DrawLine(triangleRight, triangleLeft);

            // 진행 방향 표시 (화살표, 적색)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(triangleTop, direction * triangleSize);
        }
    }
}