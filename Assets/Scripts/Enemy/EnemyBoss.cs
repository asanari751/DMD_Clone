using UnityEngine;

public class EnemyBoss : BasicEnemy
{
    public event System.Action OnBossEnemyDeath;
    private EnemyHealthBoss enemyHealthBoss;
    private EnemyHealthController enemyHealthController;

    protected override void Awake()
    {
        enemyHealthController = GetComponent<EnemyHealthController>();
        enemyHealthBoss = GetComponent<EnemyHealthBoss>();

        if (enemyHealthController != null)
        {
            enemyHealthController.OnDie += HandleOnDie;
        }
    }

    private void OnDestroy()
    {
        if (enemyHealthController != null)
        {
            enemyHealthController.OnDie -= HandleOnDie;
        }
    }

    private void HandleOnDie()
    {
        if (enemyHealthBoss != null)
        {
            enemyHealthBoss.DestroyHealthBar();
        }

        OnBossEnemyDeath?.Invoke();
    }
}