using UnityEngine;
using System.Collections;

public class B4 : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float duration;
    private float attackInterval;
    private float radius;
    private new CircleCollider2D collider;
    private bool isActive = true;
    private int skillLevel;
    private AudioManager audioManager;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.duration = skillData.duration;
        this.attackInterval = skillData.attackInterval;
        this.radius = skillData.radius;
        this.skillLevel = skillLevel;

        // 콜라이더 설정
        collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = radius;
        }

        audioManager.PlaySFX("S7");
        StartCoroutine(PullEnemies());
    }

    private IEnumerator PullEnemies()
    {
        audioManager.PlaySFX("S8");
        float elapsedTime = 0f;
        while (elapsedTime < duration && isActive)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, collider.radius, LayerMask.GetMask("Enemy"));

            foreach (Collider2D col in colliders)
            {
                EnemyHealthController enemyHealth = col.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    Vector2 directionToEnemy = transform.position - enemyHealth.transform.position;
                    float distanceToEnemy = directionToEnemy.magnitude;

                    float minDistance = 1f;
                    if (distanceToEnemy > minDistance)
                    {
                        Vector2 knockbackDirection = directionToEnemy.normalized;
                        EnemyKnockback knockback = enemyHealth.GetComponent<EnemyKnockback>();
                        if (knockback != null)
                        {
                            knockback.ApplyKnockback(knockbackDirection, Mathf.Min(knockbackForce, distanceToEnemy - minDistance));
                        }
                        enemyHealth.TakeDamage(damage, -knockbackDirection * knockbackForce);
                    }
                }
            }

            elapsedTime += attackInterval;
            yield return new WaitForSeconds(attackInterval);
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        isActive = false;
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
