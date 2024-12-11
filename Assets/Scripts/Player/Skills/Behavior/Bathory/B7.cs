using UnityEngine;

public class B7 : MonoBehaviour
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
        if (Random.value <= skillData.procChance)
        {
            float maxHealthIncrease = skillData.damage * skillLevel;
            playerHealth.IncreaseMaxHealth(maxHealthIncrease);
            DamageIndicator.Instance.ShowHeal(playerHealth.transform.position, (int)maxHealthIncrease);
            Debug.Log($"[B7 발동] {maxHealthIncrease}");
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
