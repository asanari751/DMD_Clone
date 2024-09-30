using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("애니메이션 상태 이름")]
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string walkSideStateName = "Walk_Side";
    [SerializeField] private string walkUpStateName = "Walk_Up";
    [SerializeField] private string walkDownStateName = "Walk_Down";

    private Vector2 movement;
    private string currentStateName;
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

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (GameTimerController.Paused == true) return;
        if (animator != null && spriteRenderer != null)
        {
            bool isMoving = movement.magnitude > moveThreshold;

            string newStateName;

            if (isMoving)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                {
                    newStateName = walkSideStateName;
                    spriteRenderer.flipX = (movement.x < 0);
                }
                else
                {
                    newStateName = (movement.y > 0) ? walkUpStateName : walkDownStateName;
                }
            }
            else
            {
                newStateName = idleStateName;
            }

            // 새로운 상태가 현재 상태와 다를 경우에만 재생
            if (newStateName != currentStateName)
            {
                animator.Play(newStateName, 0, 0f);
                currentStateName = newStateName;
            }
        }
    }
}
