using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillDamageArea1 : MonoBehaviour
{
    private float _damage;
    private float _knockbackForce;
    private SkillData.StatusEffectOnHit _statusEffectOnHit;
    private float _duration;
    private float _attackInterval;

    private List<EnemyHealthController> affectedEnemies = new List<EnemyHealthController>();

    public void Initialize(float damage, float knockbackForce, SkillData.StatusEffectOnHit effectOnHit, float duration, float attackInterval)
    {
        _damage = damage;
        _knockbackForce = knockbackForce;
        _statusEffectOnHit = effectOnHit;
        _duration = duration;
        _attackInterval = attackInterval;

        StartCoroutine(ApplyDamageOverTime());
    }

    private IEnumerator ApplyDamageOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            foreach (EnemyHealthController enemyHealth in affectedEnemies)
            {
                if (enemyHealth != null && !enemyHealth.IsDead())
                {
                    // 데미지 적용
                    enemyHealth.TakeDamage(_damage, Vector2.zero);

                    // 상태이상 효과 적용
                    if (_statusEffectOnHit != SkillData.StatusEffectOnHit.None)
                    {
                        EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                        if (statusEffect != null)
                        {
                            statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(_statusEffectOnHit), _duration);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(_attackInterval);
            elapsedTime += _attackInterval;
        }

        // 스킬 이펙트 제거
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
            // 필요한 경우 다른 상태이상 추가
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

    // 만약 OnTriggerEnter2D 메서드가 두 개 있었다면 하나를 삭제하세요.
    // 또한 OnTriggerExit2D 메서드도 중복되지 않도록 확인하세요.

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
