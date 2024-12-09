using UnityEngine;
using System.Collections;

public class BossAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Animation States")]
    [SerializeField] private string idleStateName = "Boss_Idle";
    [SerializeField] private string moveStateName = "Boss_Move";
    [SerializeField] private string chargeReadyStateName = "Boss_ChargeReady";
    [SerializeField] private string chargeStateName = "Boss_Charge";
    [SerializeField] private string deathStateName = "Boss_Death";

    private Vector2 movement;
    private string currentStateName;
    private bool isFacingRight = true;
    private bool isCharging = false;
    private bool isDead = false;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateMovement(Vector2 newMovement)
    {
        movement = newMovement;
        if (!isCharging && !isDead)
        {
            UpdateAnimation();
        }
    }

    public void PlayChargeReadyAnimation()
    {
        if (isDead) return;
        
        isCharging = true;
        animator.Play(chargeReadyStateName);
        currentStateName = chargeReadyStateName;
    }

    public void PlayChargeAnimation()
    {
        if (isDead) return;
        
        animator.Play(chargeStateName);
        currentStateName = chargeStateName;
    }

    public void StopCharging()
    {
        isCharging = false;
        UpdateAnimation();
    }

    public void PlayDeathAnimation()
    {
        isDead = true;
        animator.Play(deathStateName);
        currentStateName = deathStateName;
    }

    private void UpdateAnimation()
    {
        if (animator == null || spriteRenderer == null) return;
        if (isDead) return;

        bool isMoving = movement.magnitude > moveThreshold;
        string newStateName = isMoving ? moveStateName : idleStateName;

        if (movement.x != 0)
        {
            isFacingRight = movement.x > 0;
            spriteRenderer.flipX = !isFacingRight;
        }

        if (!isCharging)
        {
            animator.Play(newStateName);
            currentStateName = newStateName;
        }
    }

    public void ResetAnimationState()
    {
        currentStateName = "";
        isCharging = false;
        isDead = false;
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
