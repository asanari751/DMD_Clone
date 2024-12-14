using UnityEngine;

public class V7 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        ApplyPassiveEffect();
    }

    private void ApplyPassiveEffect()
    {
        float healthIncrease = 20f + ((skillLevel - 1) * 5f);
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        PlayerHealthUI playerHealthUI = FindAnyObjectByType<PlayerHealthUI>();

        if (playerStats != null)
        {
            float newMaxHealth = playerStats.maxHealth + healthIncrease;
            playerStats.SetMaxHealth(newMaxHealth);

            if (playerHealthUI != null)
            {
                playerHealthUI.IncreaseMaxHealth(healthIncrease);
            }
        }
    }
}