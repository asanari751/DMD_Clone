using System.Collections;
using UnityEngine;

public class S5 : MonoBehaviour
{
    private float damage;
    private float radius;
    private float duration;
    private float attackInterval;
    private SkillData.StatusEffectOnHit effectOnHit;
    private AudioManager audioManager;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        damage = skillData.damage * skillLevel;
        radius = skillData.radius;
        duration = skillData.duration;
        effectOnHit = skillData.statusEffectOnHit;
        attackInterval = skillData.attackInterval;

        // 스킬 범위 설정
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = radius;
        collider.isTrigger = true;

        // 플레이어 위치에 스킬 생성
        transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;
        audioManager.PlaySFX("S17");

        StartCoroutine(DealDamageOverTime());
    }

    private IEnumerator DealDamageOverTime()
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
            foreach (Collider2D enemy in enemies)
            {
                EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, Vector2.zero);

                    // 중독 상태효과 적용
                    EnemyStatusEffect statusEffect = enemy.GetComponent<EnemyStatusEffect>();
                    if (statusEffect != null)
                    {
                        statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Poison, duration);
                    }
                }
            }

            yield return new WaitForSeconds(attackInterval);
            elapsedTime += attackInterval;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // 중독 효과를 나타내는 초록색으로 변경
        DrawAoeRange();
    }

    private void DrawAoeRange()
    {
        float arcAngleStep = 10f;
        Vector3 center = transform.position;

        for (float angle = 0; angle < 360; angle += arcAngleStep)
        {
            float rad = angle * Mathf.Deg2Rad;
            float nextRad = (angle + arcAngleStep) * Mathf.Deg2Rad;

            Vector3 start = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector3 end = center + new Vector3(Mathf.Cos(nextRad), Mathf.Sin(nextRad)) * radius;

            Gizmos.DrawLine(start, end);
        }
    }
}
