using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private BasicEnemy basicEnemy;
    private Flowfield flowfield;
    private Rigidbody2D rb;
    private Transform playerTransform;

    private void Start()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        flowfield = FindAnyObjectByType<Flowfield>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (flowfield == null)
        {
            Debug.LogError("Flowfield를 찾을 수 없습니다!");
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 컴포넌트가 없습니다!");
        }

        if (playerTransform == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다!");
        }
    }

    private void FixedUpdate()
    {
        if (flowfield != null && rb != null && playerTransform != null && !basicEnemy.IsKnockedBack() && !basicEnemy.IsDead())
        {
            Vector2 movement = Vector2.zero;

            switch (basicEnemy.GetEnemyType())
            {
                case BasicEnemy.EnemyType.Melee:
                    movement = GetMeleeMovement();
                    break;
                case BasicEnemy.EnemyType.Arrow:
                    movement = GetArrowMovement();
                    break;
            }

            rb.velocity = movement;

            if (movement != Vector2.zero)
            {
                float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
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
