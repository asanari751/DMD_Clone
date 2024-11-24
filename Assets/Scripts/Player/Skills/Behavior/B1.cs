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
    private SkillData.StatusEffectOnHit statusEffectOnHit;
    private int skillLevel;

    private List<EnemyHealthController> affectedEnemies = new List<EnemyHealthController>();

    public void Initialize(SkillData skillData, int skillLevel)
    {
        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.radius = skillData.radius;
        this.duration = skillData.duration;
        this.attackInterval = skillData.attackInterval;
        this.statusEffectOnHit = skillData.statusEffectOnHit;
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
        float nextAttackTime = 0f;
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        while (elapsedTime < duration)
        {
            // 매 물리 업데이트마다 위치 갱신
            transform.position = playerTransform.position;

            // 공격 간격 체크
            if (Time.time >= nextAttackTime)
            {
                foreach (EnemyHealthController enemyHealth in affectedEnemies)
                {
                    if (enemyHealth != null && !enemyHealth.IsDead())
                    {
                        Vector2 knockbackDirection = (enemyHealth.transform.position - transform.position).normalized;
                        enemyHealth.TakeDamage(damage, -knockbackDirection * knockbackForce);

                        if (statusEffectOnHit != SkillData.StatusEffectOnHit.None)
                        {
                            EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                            if (statusEffect != null)
                            {
                                statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(statusEffectOnHit), duration);
                            }
                        }
                    }
                }
                nextAttackTime = Time.time + attackInterval;
                elapsedTime += attackInterval;
            }

            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }

    private EnemyStatusEffect.StatusEffectType ConvertToEnemyStatusEffectType(SkillData.StatusEffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.StatusEffectOnHit.Slow:
                return EnemyStatusEffect.StatusEffectType.Slow;
            case SkillData.StatusEffectOnHit.Bleed:
                return EnemyStatusEffect.StatusEffectType.Bleed;
            case SkillData.StatusEffectOnHit.Poison:
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
