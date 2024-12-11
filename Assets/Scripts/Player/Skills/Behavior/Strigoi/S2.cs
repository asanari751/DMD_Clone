using System.Collections;
using UnityEngine;

public class S2 : MonoBehaviour
{
    private float damage;
    private float duration;
    private float knockbackForce;
    private float attackInterval;
    private float range;
    private float angle = 180f; // 반원을 위해 180도로 설정
    private int skillLevel;
    private SkillData.StatusEffectOnHit statusEffectOnHit;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        this.damage = skillData.damage * skillLevel;
        this.duration = skillData.duration;
        this.knockbackForce = skillData.knockbackForce;
        this.attackInterval = skillData.attackInterval;
        this.range = skillData.range;
        this.skillLevel = skillLevel;
        this.statusEffectOnHit = skillData.statusEffectOnHit;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;
        transform.SetParent(playerTransform);

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, skillData.range, LayerMask.GetMask("Enemy"));
        if (enemies.Length == 0)
        {
            Destroy(gameObject);
            return;
        }


        StartCoroutine(ExecuteSkill());
    }

    private IEnumerator ExecuteSkill()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0)
        {
            Collider2D nearestEnemy = null;
            float nearestDistance = range;

            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, targetAngle);

                PerformAttack();
            }
        }

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private void PerformAttack()
    {
        Vector2 attackDirection = transform.right;
        int hitCount = 0;

        // range는 이미 ExecuteSkill에서 체크했으므로 여기서는 angle 체크만 수행
        foreach (Collider2D enemy in Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy")))
        {
            Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float angleToEnemy = Vector2.Angle(attackDirection, directionToEnemy);

            if (angleToEnemy <= angle / 2)
            {
                EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemyHealth.TakeDamage(damage, knockbackDir * -knockbackForce);

                    var enemyStatusEffect = enemy.GetComponent<EnemyStatusEffect>();
                    if (enemyStatusEffect != null)
                    {
                        enemyStatusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(statusEffectOnHit), duration);
                    }

                    hitCount++;
                }
            }
        }

        if (hitCount > 0)
        {
            var playerHealthUI = GetComponentInParent<PlayerHealthUI>();
            if (playerHealthUI != null)
            {
                playerHealthUI.Heal(hitCount);

                if (DamageIndicator.Instance != null)
                {
                    Vector3 playerPosition = transform.position;
                    DamageIndicator.Instance.ShowHeal(playerPosition, hitCount);
                }
            }
        }
    }

    private EnemyStatusEffect.StatusEffectType ConvertToEnemyStatusEffectType(SkillData.StatusEffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.StatusEffectOnHit.Fear:
                return EnemyStatusEffect.StatusEffectType.Fear;
            case SkillData.StatusEffectOnHit.Bleed:
                return EnemyStatusEffect.StatusEffectType.Bleed;
            case SkillData.StatusEffectOnHit.Slow:
                return EnemyStatusEffect.StatusEffectType.Slow;
            case SkillData.StatusEffectOnHit.Weakness:
                return EnemyStatusEffect.StatusEffectType.Weakness;
            case SkillData.StatusEffectOnHit.Poison:
                return EnemyStatusEffect.StatusEffectType.Poison;
            case SkillData.StatusEffectOnHit.Stun:
                return EnemyStatusEffect.StatusEffectType.Stun;
            default:
                return EnemyStatusEffect.StatusEffectType.None;
        }
    }

    // =================================

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.right;

        Vector3 leftDirection = Quaternion.Euler(0, 0, angle / 2) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -angle / 2) * direction;

        Gizmos.DrawLine(transform.position, transform.position + leftDirection * range);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * range);

        float arcAngleStep = 5f;
        for (float i = -angle / 2; i <= angle / 2; i += arcAngleStep)
        {
            float start = i;
            float end = i + arcAngleStep;

            Vector3 startPos = transform.position + Quaternion.Euler(0, 0, start) * direction * range;
            Vector3 endPos = transform.position + Quaternion.Euler(0, 0, end) * direction * range;

            Gizmos.DrawLine(startPos, endPos);
        }
    }
}