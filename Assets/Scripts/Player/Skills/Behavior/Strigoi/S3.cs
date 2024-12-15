using UnityEngine;
using System.Collections;

public class S3 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float damage;
    private float duration;
    private float radius;
    private float attackInterval;
    private LayerMask enemyLayer;
    private AudioManager audioManager;

    public void Initialize(SkillData data, int level)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        skillData = data;
        skillLevel = level;
        enemyLayer = LayerMask.GetMask("Enemy");
        
        // 레벨별 데미지와 지속시간 설정
        damage = skillData.damage + ((skillLevel - 1) * 10f);
        duration = skillLevel == 3 ? skillData.duration + 1f : skillData.duration;
        radius = skillData.radius;
        attackInterval = skillData.attackInterval;
        
        transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;
        
        audioManager.PlaySFX("S18");
        StartCoroutine(DamageOverTime());
        Destroy(gameObject, duration);
    }

    private IEnumerator DamageOverTime()
    {
        float elapsedTime = 0f;
        float nextAttackTime = 0f;
        
        while (elapsedTime < duration)
        {
            if (elapsedTime >= nextAttackTime)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
                
                foreach (Collider2D enemy in enemies)
                {
                    EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
                    EnemyStatusEffect enemyStatus = enemy.GetComponent<EnemyStatusEffect>();
                    
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage, Vector2.zero);
                        
                        if (enemyStatus != null && !enemyStatus.HasStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness))
                        {
                            enemyStatus.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness, 8f);
                        }
                    }
                }
                nextAttackTime = elapsedTime + attackInterval;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}