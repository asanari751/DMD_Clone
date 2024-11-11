using UnityEngine;
using System.Collections;

public class IronMaiden : MonoBehaviour
{
    [SerializeField] private float stopRadius = 1f;

    private float damage;
    private float pullForce;
    private float duration;
    private float attackInterval;

    public void Initialize(float damage, float pullForce, float duration, float attackInterval)
    {
        this.damage = damage;
        this.pullForce = pullForce;
        this.duration = duration;
        this.attackInterval = attackInterval;
        StartCoroutine(PullEnemies());
    }

    private IEnumerator PullEnemies()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, GetComponent<CircleCollider2D>().radius);

            foreach (Collider2D collider in colliders)
            {
                BasicEnemy enemy = collider.GetComponent<BasicEnemy>();
                if (enemy != null)
                {
                    Vector2 directionToEnemy = enemy.transform.position - transform.position;
                    float distanceToEnemy = directionToEnemy.magnitude;

                    // 최소 거리 설정 (아이언 메이든과 너무 가까워지지 않도록)
                    float minDistance = 1f;
                    if (distanceToEnemy > minDistance)
                    {
                        Vector2 pullDirection = -directionToEnemy.normalized;
                        EnemyKnockback knockback = enemy.GetComponent<EnemyKnockback>();
                        if (knockback != null)
                        {
                            knockback.SetKnockbackDistance(Mathf.Min(pullForce, distanceToEnemy - minDistance));
                        }
                        enemy.TakeDamage(damage, -pullDirection);
                    }
                }
            }

            elapsedTime += attackInterval;
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void OnDrawGizmos()
    {
        if (GetComponent<CircleCollider2D>() != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>().radius);
        }
    }
}