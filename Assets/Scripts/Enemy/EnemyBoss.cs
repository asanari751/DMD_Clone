using UnityEngine;

public class EnemyBoss : BasicEnemy
{
    public event System.Action OnBossEnemyDeath;

    public override void Die()
    {
        if (!IsDead())
        {
            base.Die();
            OnBossEnemyDeath?.Invoke();
        }
    }
}