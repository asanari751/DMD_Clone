using Unity.VisualScripting;
using UnityEngine;

public class EnemyElite : BasicEnemy
{
    public event System.Action OnEliteEnemyDeath;
    private EnemyHealthController enemyHealthController;
    private EnemyHealthElite enemyHealthElite;

    protected override void Awake()
    {
        base.Awake();
        
        enemyHealthController = GetComponent<EnemyHealthController>();
        enemyHealthElite = GetComponent<EnemyHealthElite>();

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
        enemyHealthElite.DestroyHealthBar();
        OnEliteEnemyDeath?.Invoke();
    }
}