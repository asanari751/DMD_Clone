using UnityEngine;

public class V2Damage : MonoBehaviour
{
    private float damage;
    private float stunDuration;

    public void Initialize(float damage, float stunDuration)
    {
        this.damage = damage;
        this.stunDuration = stunDuration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, Vector2.zero);
            }

            EnemyStatusEffect statusEffect = other.GetComponent<EnemyStatusEffect>();
            if (statusEffect != null)
            {
                statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Stun, stunDuration);
            }
        }
    }
}
