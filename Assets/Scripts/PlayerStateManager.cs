using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Linq;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float attackAngle = 180f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackEffectDuration = 0.5f;
    [SerializeField] private SpriteRenderer attackEffectSprite;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private float atkDelay = 1f;

    private Camera mainCamera;
    private bool isAutoAttack = false;
    private Tween autoAttackTween;
    private float lastAttackTime = 0f;

    private void Awake()
    {
        attackAction.action.performed += _ => ToggleAutoAttack();
        mainCamera = Camera.main;
    }

    private void OnEnable() => attackAction.action.Enable();

    private void OnDisable()
    {
        attackAction.action.Disable();
        StopAutoAttack();
    }

    private void ToggleAutoAttack()
    {
        isAutoAttack = !isAutoAttack;
        if (isAutoAttack) StartAutoAttack();
        else StopAutoAttack();
    }

    private void StartAutoAttack()
    {
        StopAutoAttack();
        autoAttackTween = DOVirtual.DelayedCall(atkDelay, PerformAttack, false).SetLoops(-1);
    }

    private void StopAutoAttack() => DOTween.Kill(autoAttackTween);

    private void PerformAttack()
    {
        if (Time.time - lastAttackTime < atkDelay) return;

        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            AttackInSemicircle(closestEnemy.transform.position);
            lastAttackTime = Time.time;
        }
    }

    private GameObject FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayers);
        return colliders.Length > 0 ? 
            colliders.OrderBy(c => Vector2.Distance(transform.position, c.transform.position)).First().gameObject : 
            null;
    }

    private void AttackInSemicircle(Vector2 targetPosition)
    {
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayers))
        {
            Vector2 directionToCollider = (collider.transform.position - transform.position).normalized;
            float angleToCollider = Mathf.Atan2(directionToCollider.y, directionToCollider.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(Mathf.DeltaAngle(angleToTarget, angleToCollider)) <= attackAngle / 2)
            {
                if (collider.TryGetComponent<BasicEnemy>(out var enemy))
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeDamage(attackDamage, knockbackDirection);
                }
            }
        }
        ShowAttackEffect(targetPosition);
    }

    private void ShowAttackEffect(Vector2 targetPosition)
    {
        if (attackEffectSprite == null) return;

        DOTween.Kill(attackEffectSprite);
        attackEffectSprite.gameObject.SetActive(true);
        attackEffectSprite.color = Color.white;

        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        Vector2 originalSize = attackEffectSprite.sprite.bounds.size;
        float scaleX = detectionRadius * 2 / originalSize.x;
        float scaleY = detectionRadius / originalSize.y;

        attackEffectSprite.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        attackEffectSprite.transform.position = (Vector2)transform.position + directionToTarget * (detectionRadius / 2f);
        attackEffectSprite.transform.rotation = Quaternion.Euler(0, 0, angleToTarget - 90);

        DOTween.Sequence()
            .Append(attackEffectSprite.DOFade(0.5f, attackEffectDuration * 0.1f))
            .Append(attackEffectSprite.DOFade(0f, attackEffectDuration * 0.4f))
            .OnComplete(() => attackEffectSprite.gameObject.SetActive(false));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            AttackInSemicircle(worldPosition);
            lastAttackTime = Time.time;
        }
    }
}