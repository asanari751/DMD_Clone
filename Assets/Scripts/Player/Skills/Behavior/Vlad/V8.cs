using UnityEngine;

public class V8 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;

    private readonly float[] criticalChanceIncreases = { 0.05f, 0.07f, 0.09f, 0.13f, 0.15f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        ApplyPassiveEffect();
    }

    private void ApplyPassiveEffect()
    {
        float criticalChanceIncrease = criticalChanceIncreases[skillLevel - 1];
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();

        if (playerStats != null)
        {
            float newCriticalChance = playerStats.criticalChance + criticalChanceIncrease;
            playerStats.SetCriticalChance(newCriticalChance);
        }
    }
}
