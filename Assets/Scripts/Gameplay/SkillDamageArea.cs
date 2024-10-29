using UnityEngine;

public class SkillDamageArea : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    
    public void Initialize(float damage, float knockbackForce)
    {
        this.damage = damage;
        this.knockbackForce = knockbackForce;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            damageable.TakeDamage(damage, knockbackDirection * knockbackForce);
            
            // 데미지 인디케이터 표시
            DamageIndicator.Instance.ShowDamage(other.transform.position, (int)damage);
        }
    }
}