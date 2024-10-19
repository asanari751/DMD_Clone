using UnityEngine;
using System.Collections;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Animation States")]
    [SerializeField] private string idleStateName = "Enemy_Idle";
    [SerializeField] private string walkSideStateName = "Enemy_Side";
    [SerializeField] private string attackStateName = "Enemy_Attack";

    private Vector2 movement;
    private string currentStateName;
    private bool isFacingRight = true;
    private bool isAttacking = false;
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
        if (!isAttacking)
        {
            UpdateAnimation();
        }
    }

    public void PlayAttackAnimation()
    {
        if (!isAttacking)
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

    private void UpdateAnimation()
    {
        if (animator != null && spriteRenderer != null)
        {
            string newStateName;

            bool isMoving = movement.magnitude > moveThreshold;
            newStateName = isMoving ? walkSideStateName : idleStateName;

            if (movement.x != 0)
            {
                isFacingRight = movement.x > 0;
            }

            spriteRenderer.flipX = !isFacingRight;

            if (newStateName != currentStateName)
            {
                Debug.Log($"Playing animation: {newStateName}");
                animator.Play(newStateName, 0, 0f);
                currentStateName = newStateName;
            }
        }
    }

    public void ResetAnimationState()
    {
        currentStateName = "";
        isAttacking = false;
        UpdateAnimation();
    }
}