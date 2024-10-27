using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Animation States")]
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string walkSideStateName = "Walk_Side";
    [SerializeField] private string deathStateName = "Death";
    // [SerializeField] private string walkUpStateName = "Walk_Up";
    // [SerializeField] private string walkDownStateName = "Walk_Down";

    private Vector2 movement;
    private string currentStateName;
    private bool lastHorizontalDirectionWasLeft = false;
    private bool isDead = false;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(movement.x) > 0)
        {
            lastHorizontalDirectionWasLeft = movement.x < 0;
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (PauseController.Paused == true || isDead) return;
        if (animator != null && spriteRenderer != null)
        {
            bool isMoving = movement.magnitude > moveThreshold;

            string newStateName;

            if (isMoving)
            {
                newStateName = walkSideStateName;

                if (Mathf.Abs(movement.x) > 0)
                {
                    spriteRenderer.flipX = (movement.x < 0);
                }
                else
                {
                    spriteRenderer.flipX = lastHorizontalDirectionWasLeft;
                }

                // if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                // {
                //     newStateName = walkSideStateName;
                //     spriteRenderer.flipX = (movement.x < 0);
                // }
                // else
                // {
                //     newStateName = (movement.y > 0) ? walkUpStateName : walkDownStateName;
                // }
            }

            else
            {
                newStateName = idleStateName;
            }

            if (newStateName != currentStateName)
            {
                animator.Play(newStateName, 0, 0f);
                currentStateName = newStateName;
            }
        }
    }

    public void PlayDeathAnimation()
    {
        isDead = true;
        if (animator != null)
        {
            animator.Play(deathStateName);
        }
    }

    public float GetDeathAnimationLength() // PlayerHealthUI.Die()
    {
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.length;
            }
        }
        return 1f;
    }
}
