using UnityEngine;
using System.Collections;

public class S7 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float checkInterval = 1f;
    private float poisonDamage = 1f;
    private readonly float[] cooldowns = { 14f, 13f, 12f, 11f, 10f };
    private readonly float[] damages = { 50f, 75f, 75f, 100f, 100f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        skillData.cooldown = cooldowns[skillLevel - 1];
        StartCoroutine(ExecuteSkill());
    }

    private IEnumerator ExecuteSkill()
    {
        var enemies = FindObjectsByType<EnemyStatusEffect>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.HasStatusEffect(EnemyStatusEffect.StatusEffectType.Poison) ||
                enemy.HasStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness))
            {
                var healthController = enemy.GetComponent<EnemyHealthController>();
                if (healthController != null)
                {
                    healthController.TakeDamage(damages[skillLevel - 1], Vector2.zero);
                }

                enemy.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Poison, enemy.poisonDebuff);
            }
        }

        Destroy(gameObject);
        yield break;
    }
}
