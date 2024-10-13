using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private BasicEnemy basicEnemy;
    private EnemyStats enemyStats;
    private Flowfield flowfield;
    private Rigidbody2D rb;
    private Transform playerTransform;

    private void Start()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        flowfield = FindAnyObjectByType<Flowfield>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void FixedUpdate()
    {
        if (flowfield != null && rb != null && playerTransform != null && !basicEnemy.IsKnockedBack() && !basicEnemy.IsDead())
        {
            // 공격 중이거나 넉백 상태면 이동하지 않음
            if (basicEnemy.IsAttacking || basicEnemy.IsKnockedBack())
            {
                rb.linearVelocity = Vector2.zero;
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
            basicEnemy.CheckAttack();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
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
}
