using UnityEngine;
using System.Collections;

public class B2 : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float range;
    private float angle;
    private int skillLevel;
    private SkillData.EffectOnHit effectOnHit;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.range = skillData.range;
        this.angle = skillData.angle;
        this.skillLevel = skillLevel;
        this.effectOnHit = skillData.effectOnHit;

        // 플레이어 위치에 스킬 배치
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;

        // 부채꼴 범위 내 적 감지 및 데미지 처리
        DetectAndDamageEnemies();
    }

    private void DetectAndDamageEnemies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));

        foreach (Collider2D col in colliders)
        {
            Vector2 directionToTarget = (col.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(transform.right, directionToTarget);

            if (angleToTarget <= angle / 2)
            {
                EnemyHealthController enemyHealth = col.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    // 넉백 방향 설정
                    Vector2 knockbackDirection = directionToTarget;

                    // 데미지와 넉백 적용
                    enemyHealth.TakeDamage(damage, knockbackDirection * knockbackForce);

                    // 상태이상 효과 적용
                    if (effectOnHit != SkillData.EffectOnHit.None)
                    {
                        EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                        if (statusEffect != null)
                        {
                            statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(effectOnHit), 2f);
                        }
                    }
                }
            }
        }

        // 스킬 이펙트 제거 (약간의 지연 후)
        Destroy(gameObject, 0.5f);
    }

    private EnemyStatusEffect.StatusEffectType ConvertToEnemyStatusEffectType(SkillData.EffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.EffectOnHit.Slow:
                return EnemyStatusEffect.StatusEffectType.Slow;
            case SkillData.EffectOnHit.Bleed:
                return EnemyStatusEffect.StatusEffectType.Bleed;
            case SkillData.EffectOnHit.Poison:
                return EnemyStatusEffect.StatusEffectType.Poison;
            default:
                return EnemyStatusEffect.StatusEffectType.None;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.right;
        Vector3 leftDirection = Quaternion.Euler(0, 0, angle / 2) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -angle / 2) * direction;

        Gizmos.DrawLine(transform.position, transform.position + leftDirection * range);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * range);

        DrawArc();
    }

    private void DrawArc() // 헬퍼
    {
        Vector3 center = transform.position;
        float arcAngleStep = 1f;

        for (float i = -angle / 2; i <= angle / 2; i += arcAngleStep)
        {
            float startAngle = i;
            float endAngle = i + arcAngleStep;

            Vector3 start = center + Quaternion.Euler(0, 0, startAngle) * transform.right * range;
            Vector3 end = center + Quaternion.Euler(0, 0, endAngle) * transform.right * range;

            Gizmos.DrawLine(start, end);
        }
    }
}
