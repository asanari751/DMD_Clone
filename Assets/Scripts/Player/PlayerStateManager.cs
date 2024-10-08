using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Linq;

public class PlayerStateManager : MonoBehaviour
{
    public enum AttackType
    {
        Melee,
        Arrow
    }

    [Header("Common")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float detectionRadius;
    [SerializeField] private AttackType currentAttackType;
    [SerializeField] private float atkDelay;

    [Header("Melee")]
    [SerializeField] private float attackAngle;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackEffectDuration;
    [SerializeField] private float meeleAttackRange;

    [Header("Arrow")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileDamage;
    [SerializeField] private int penetrateCount;

    [Header("Projectile Trail")]
    [SerializeField] private TrailRenderer projectileTrailPrefab;
    [SerializeField] private ProjectileTrailSettings projectileTrailSettings;

    [Header("Attack Effect")]
    [SerializeField] private SpriteRenderer attackEffectSprite;
    [SerializeField] private Vector2 attackEffectOffset = Vector2.zero;

    private Camera mainCamera;
    private bool isAutoAttack = false;
    private Tween autoAttackTween;
    private float lastAttackTime = 0f;
    private float lastRangedAttackTime = 0f;
    public event Action<float, float, int> OnStatsUpdated;

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

    private void OnEnable() => attackAction.action.Enable();
    private void OnDisable() => attackAction.action.Disable();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformManualAttack();
        }
    }

    private void ToggleAutoAttack()
    {
        isAutoAttack = !isAutoAttack;
        if (isAutoAttack)
        {
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
        } return;
    }

    private void PerformManualAttack()
    {
        if (PauseController.Paused == true) return;

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Action<Vector2> attackAction = currentAttackType == AttackType.Arrow ? PerformRangedAttack : PerformAttack;
        attackAction(mousePosition);
    }

    private void PerformRangedAttack(Vector2 targetPosition)
    {
        if (Time.time - lastRangedAttackTime < atkDelay)
        {
            return;
        }

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        GameObject projectileObj = projectilePool.GetProjectile();
        projectileObj.transform.position = transform.position;
        projectileObj.transform.right = direction;

        Projectile projectile = projectileObj.GetComponent<Projectile>();

        TrailRenderer trail = projectileObj.GetComponent<TrailRenderer>();

        trail.time = projectileTrailSettings.trailDuration;
        trail.startWidth = projectileTrailSettings.trailStartWidth;
        trail.endWidth = projectileTrailSettings.trailEndWidth;
        trail.startColor = projectileTrailSettings.trailStartColor;
        trail.endColor = projectileTrailSettings.trailEndColor;
        trail.enabled = true;

        projectile.Initialize(projectileSpeed, projectileDamage, penetrateCount, projectilePool);

        lastRangedAttackTime = Time.time;
    }

    private void PerformAttack(Vector2 targetPosition)
    {
        if (Time.time - lastAttackTime < atkDelay)
        {
            return;
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
            if (IsInAttackAngle(collider.transform.position, angleToTarget) &&
                collider.TryGetComponent<BasicEnemy>(out var enemy))
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(attackDamage, knockbackDirection);
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
        if (attackEffectSprite == null) return;

        DOTween.Kill(attackEffectSprite);
        attackEffectSprite.gameObject.SetActive(true);
        attackEffectSprite.color = Color.white;

        SetAttackEffectTransform(targetPosition);

        DOTween.Sequence()
            .Append(attackEffectSprite.DOFade(0.5f, attackEffectDuration * 0.1f))
            .Append(attackEffectSprite.DOFade(0f, attackEffectDuration * 0.4f))
            .OnComplete(() => attackEffectSprite.gameObject.SetActive(false));
    }

    private void SetAttackEffectTransform(Vector2 targetPosition)
    {
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        Vector2 originalSize = attackEffectSprite.sprite.bounds.size;
        float scaleX = meeleAttackRange * 2 / originalSize.x;
        float scaleY = meeleAttackRange / originalSize.y;

        attackEffectSprite.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        Vector2 adjustedOffset = CalculateDirectionalOffset(directionToTarget);

        Vector3 basePosition = transform.position + (Vector3)adjustedOffset;
        attackEffectSprite.transform.position = (Vector2)basePosition + directionToTarget * (meeleAttackRange / 2f);

        attackEffectSprite.transform.rotation = Quaternion.Euler(0, 0, angleToTarget - 90);
    }

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

    private void UpdateStats()
    {
        OnStatsUpdated?.Invoke(attackDamage, projectileDamage, penetrateCount);
    }
}