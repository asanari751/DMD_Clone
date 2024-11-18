using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class B1 : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float radius;
    private float duration;
    private float attackInterval;
    private SkillData.EffectOnHit effectOnHit;
    private int skillLevel;

    private List<EnemyHealthController> affectedEnemies = new List<EnemyHealthController>();

    public void Initialize(SkillData skillData, int skillLevel)
    {
        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.radius = skillData.radius;
        this.duration = skillData.duration;
        this.attackInterval = skillData.attackInterval;
        this.effectOnHit = skillData.effectOnHit;
        this.skillLevel = skillLevel;

        // 스킬 이펙트를 플레이어 위치에 배치
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;

        // 스킬 범위 설정
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = radius;
        collider.isTrigger = true;

        // 스킬 작동 시작
        StartCoroutine(ApplyDamageOverTime());
    }

    private IEnumerator ApplyDamageOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            foreach (EnemyHealthController enemyHealth in affectedEnemies)
            {
                if (enemyHealth != null && !enemyHealth.IsDead())
                {
                    // 스킬 중심점에서 적 방향으로 향하는 벡터 (밀어내는 방향)
                    Vector2 knockbackDirection = (enemyHealth.transform.position + transform.position).normalized;

                    // 데미지와 넉백 적용
                    enemyHealth.TakeDamage(damage, knockbackDirection * knockbackForce);

                    // 상태이상 효과 적용
                    if (effectOnHit != SkillData.EffectOnHit.None)
                    {
                        EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                        if (statusEffect != null)
                        {
                            statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(effectOnHit), duration);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(attackInterval);
            elapsedTime += attackInterval;
        }

        // 스킬 이펙트 제거
        Destroy(gameObject);
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
            // 필요한 경우 다른 상태이상 효과 추가
            default:
                return EnemyStatusEffect.StatusEffectType.None;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = collision.GetComponent<EnemyHealthController>();
            if (enemyHealth != null && !affectedEnemies.Contains(enemyHealth))
            {
                affectedEnemies.Add(enemyHealth);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = collision.GetComponent<EnemyHealthController>();
            if (enemyHealth != null && affectedEnemies.Contains(enemyHealth))
            {
                affectedEnemies.Remove(enemyHealth);
            }
        }
    }
}
