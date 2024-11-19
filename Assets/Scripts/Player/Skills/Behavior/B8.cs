using UnityEngine;

public class B8 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private PlayerHealthUI playerHealth;
    private EnemySpawner enemySpawner;

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        playerHealth = Object.FindFirstObjectByType<PlayerHealthUI>();
        enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyDefeated += OnEnemyKilled;
        }
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        // 현재 체력이 최대 체력과 같다면 스킬을 발동하지 않음
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return;

        if (Random.value <= skillData.procChance)
        {
            float healAmount = skillData.damage * skillLevel;
            playerHealth.Heal(healAmount);
            DamageIndicator.Instance.ShowHeal(playerHealth.transform.position, (int)healAmount);
            Debug.Log($"[B8 발동] {healAmount}");
        }
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyDefeated -= OnEnemyKilled;
        }
    }
}
