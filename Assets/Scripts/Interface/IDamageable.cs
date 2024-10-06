using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector2 knockbackDirection);
    bool IsDead();
}
