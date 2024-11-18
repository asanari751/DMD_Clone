using UnityEngine;

public class B3 : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float range;
    private float projectileSpeed = 15f;
    private int skillLevel;
    private SkillData.EffectOnHit effectOnHit;
    private Vector2 direction;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        this.damage = skillData.damage * skillLevel;
        this.knockbackForce = skillData.knockbackForce;
        this.range = skillData.range;
        this.skillLevel = skillLevel;
        this.effectOnHit = skillData.effectOnHit;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;

        // 마우스 위치로 발사 방향 설정
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = (mousePosition - (Vector2)transform.position).normalized;
        
        // 화살 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        StartProjectile();
    }

    private void StartProjectile()
    {
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = direction * projectileSpeed;

        // 최대 사거리에 도달하면 제거
        Destroy(gameObject, range / projectileSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = collision.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                // 데미지와 넉백 적용
                enemyHealth.TakeDamage(damage, direction * knockbackForce);

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
}
