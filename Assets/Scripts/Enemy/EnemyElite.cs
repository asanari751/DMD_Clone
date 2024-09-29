using UnityEngine;

public class EliteEnemy : BasicEnemy
{
    public event System.Action OnEliteEnemyDeath;
    public override void Die()
    {
        if (!IsDead())
        {
            base.Die();
            OnEliteEnemyDeath?.Invoke();
        }
    }
}