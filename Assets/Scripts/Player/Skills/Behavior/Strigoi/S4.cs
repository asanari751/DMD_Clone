using UnityEngine;
using System.Collections;

public class S4 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float damage;
    private float radius;
    private float knockbackForce;
    private float duration;
    private float attackInterval;
    private bool isActive = true;
    private float activationDelay = 1f;
    private float statusEffectDuration = 8f;
    private AudioManager audioManager;

    private readonly float[] damageValues = { 30f, 45f, 60f };
    private readonly float[] radiusValues = { 5f, 5.5f, 6f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        int index = skillLevel - 1;

        damage = damageValues[index];
        radius = radiusValues[index];
        knockbackForce = skillData.knockbackForce;
        duration = skillData.duration;
        attackInterval = skillData.attackInterval;
        audioManager = FindAnyObjectByType<AudioManager>();

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(playerTransform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            transform.position = nearestEnemy.transform.position;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        audioManager.PlaySFX("S19");
        StartCoroutine(ActivateAfterDelay());
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        StartCoroutine(PullEnemies());
    }

    private IEnumerator PullEnemies()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration && isActive)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    EnemyHealthController enemyHealth = col.GetComponent<EnemyHealthController>();
                    if (enemyHealth != null)
                    {
                        Vector2 directionToTombstone = (transform.position - col.transform.position).normalized;
                        float distanceToEnemy = Vector2.Distance(transform.position, col.transform.position);

                        // 끌어당기기 효과
                        EnemyKnockback knockback = col.GetComponent<EnemyKnockback>();
                        if (knockback != null)
                        {
                            knockback.ApplyKnockback(-directionToTombstone, knockbackForce);
                        }

                        // 데미지와 상태이상 적용
                        enemyHealth.TakeDamage(damage, -directionToTombstone * knockbackForce);

                        EnemyStatusEffect statusEffect = col.GetComponent<EnemyStatusEffect>();
                        if (statusEffect != null)
                        {
                            statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness, statusEffectDuration);
                            statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Poison, statusEffectDuration + 2f);
                        }
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
        if (radius > 0)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
