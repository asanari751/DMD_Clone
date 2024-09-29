using UnityEngine;

public class EnemyBoss : BasicEnemy
{
    public event System.Action OnBossEnemyDeath;

    public override void Die()
    {
        if (!IsDead())
        {
            Debug.Log("Enemy Boss Died!");
            base.Die();
            OnBossEnemyDeath?.Invoke();
        }
    }
}