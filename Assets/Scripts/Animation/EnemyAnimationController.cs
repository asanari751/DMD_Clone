using UnityEngine;
using System.Collections;

// ================================
// 우선순위 : Hit > Attack > Move > Idle
// ================================

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Animation States")]
    [SerializeField] private string idleStateName = "Enemy_Idle";
    [SerializeField] private string walkSideStateName = "Enemy_Side";
    [SerializeField] private string attackStateName = "Enemy_Attack";
    [SerializeField] private string hitStateName = "Enemy_Hit";
    [SerializeField] private float hitAnimationDuration;

    private Vector2 movement;
    private string currentStateName;
    private bool isFacingRight = true;
    private bool isAttacking = false;
    private bool isHit = false;
    private BasicEnemy basicEnemy;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        basicEnemy = GetComponent<BasicEnemy>();
    }


    public void UpdateMovement(Vector2 newMovement)
    {
        movement = newMovement;
        if (!isAttacking && !isHit)
        {
            UpdateAnimation();
        }
    }

    public void PlayAttackAnimation()
    {
        if (!isAttacking && !isHit)
        {
            isAttacking = true;
            StartCoroutine(AttackAnimationCoroutine());
        }
    }

    private IEnumerator AttackAnimationCoroutine()
    {
        animator.Play(attackStateName, 0, 0f);
        currentStateName = attackStateName;
        Debug.Log($"Playing attack animation: {attackStateName}");

        yield return new WaitForSeconds(basicEnemy.GetAttackDelay());

        isAttacking = false;
        UpdateAnimation();
    }

    public void PlayHitAnimation()
    {
        isHit = true;
        StartCoroutine(HitAnimationCoroutine());
    }

    private IEnumerator HitAnimationCoroutine()
    {
        if (animator.HasState(0, Animator.StringToHash(hitStateName)))
        {
            animator.Play(hitStateName, 0, 0f);
            currentStateName = hitStateName;
        }
        else
        {
            animator.Play(idleStateName, 0, 0f);
            currentStateName = idleStateName;
        }

        Debug.Log($"Playing Hit animation: {hitStateName}");

        yield return new WaitForSeconds(hitAnimationDuration);

        isHit = false;
        isAttacking = false;
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator != null && spriteRenderer != null)
        {
            bool isMoving = movement.magnitude > moveThreshold;
            string newStateName = isMoving ? walkSideStateName : idleStateName;

            if (movement.x != 0)
            {
                isFacingRight = movement.x > 0;
                spriteRenderer.flipX = !isFacingRight;
            }

            animator.Play(newStateName);
            currentStateName = newStateName;
        }
    }

    public void ResetAnimationState()
    {
        // 모든 상태 리셋
        currentStateName = "";
        isAttacking = false;
        isHit = false;
        isFacingRight = true;
        movement = Vector2.zero;

        if (animator != null)
        {
            animator.Play(idleStateName);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
    }
}