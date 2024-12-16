using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private BasicEnemy basicEnemy;
    private EnemyStatusEffect enemyStatusEffect;
    private EnemyHealthController enemyHealthController;
    private EnemyStats enemyStats;
    private Flowfield flowfield;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private EnemyAnimationController animationController;
    private PlayerHealthUI playerHealth;

    private void Start()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        enemyStatusEffect = GetComponent<EnemyStatusEffect>();
        enemyHealthController = GetComponent<EnemyHealthController>();
        flowfield = FindAnyObjectByType<Flowfield>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        animationController = GetComponent<EnemyAnimationController>();
        playerHealth = playerTransform?.GetComponent<PlayerHealthUI>();
    }

    private void FixedUpdate()
    {
        if (flowfield != null && rb != null && playerTransform != null &&
                   !basicEnemy.IsKnockedBack() && !enemyHealthController.IsDead() &&
                   !playerHealth.IsDead())
        {
            BossAnimationController bossAnimController = GetComponent<BossAnimationController>();
            if (!basicEnemy.CanMove())
            {
                rb.linearVelocity = Vector2.zero;
                if (bossAnimController != null)
                    bossAnimController.UpdateMovement(Vector2.zero);
                else if (animationController != null)
                    animationController.UpdateMovement(Vector2.zero);
                return;
            }

            // 공포 상태일 때의 이동 처리
            if (enemyStatusEffect.IsFeared())
            {
                Vector2 directionFromPlayer = (transform.position - playerTransform.position).normalized;
                Vector2 fearMovement = directionFromPlayer * basicEnemy.GetMoveSpeed() / 2f;
                rb.linearVelocity = fearMovement;
                if (animationController != null) animationController.UpdateMovement(fearMovement);
                return;
            }

            Vector2 movement = Vector2.zero;

            switch (basicEnemy.GetEnemyType())
            {
                case EnemyStats.EnemyType.Melee:
                    movement = GetMeleeMovement();
                    break;
                case EnemyStats.EnemyType.Arrow:
                    movement = GetArrowMovement();
                    break;
            }

            rb.linearVelocity = movement;

            if (bossAnimController != null)
                bossAnimController.UpdateMovement(movement);
            else if (animationController != null)
                animationController.UpdateMovement(movement);

            basicEnemy.CheckAttack();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (animationController != null) animationController.UpdateMovement(Vector2.zero);
        }
    }

    private Vector2 GetMeleeMovement()
    {
        Vector2 flowDirection = flowfield.GetFlowDirection(transform.position);
        return flowDirection * basicEnemy.GetMoveSpeed();
    }

    private Vector2 GetArrowMovement()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (Mathf.Abs(distanceToPlayer - basicEnemy.GetAttackRange()) < 0.1f)
        {
            return Vector2.zero;
        }
        else if (distanceToPlayer > basicEnemy.GetAttackRange())
        {
            Vector2 flowDirection = flowfield.GetFlowDirection(transform.position);
            return flowDirection * basicEnemy.GetMoveSpeed();
        }
        else
        {
            return -directionToPlayer * basicEnemy.GetRetreatSpeed();
        }
    }

    public void ResetMovementController()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        flowfield = FindAnyObjectByType<Flowfield>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        animationController = GetComponent<EnemyAnimationController>();
    }
}