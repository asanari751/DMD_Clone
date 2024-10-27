using UnityEngine;

public class EnemyBoss : BasicEnemy
{
    public event System.Action OnBossEnemyDeath;
    private GameTimerController gameTimerController;

    private new void Start()
    {
        base.Start(); // 부모 클래스의 초기화 추가
        gameTimerController = FindAnyObjectByType<GameTimerController>();
    }

    public override void Die()
    {
        if (!IsDead())
        {
            base.Die();
            OnBossEnemyDeath?.Invoke();
            gameTimerController.TriggerBossDefeated();
        }
    }
}