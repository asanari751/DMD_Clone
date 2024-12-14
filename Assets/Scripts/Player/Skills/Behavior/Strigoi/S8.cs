using UnityEngine;
using System.Collections;

public class S8 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private PlayerStateManager playerStateManager;
    private PlayerStats playerStats;
    private bool isBuffActive = false;

    private readonly float[] damageIncreases = { 1.05f, 1.07f, 1.10f, 1.12f, 1.15f };
    private readonly float[] buffDurations = { 5f, 5f, 6f, 6f, 7f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        playerStats = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None)[0];

        var enemies = FindObjectsByType<EnemyHealthController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.OnDie += CheckEnemyStatusOnDeath;
        }
    }

    private void CheckEnemyStatusOnDeath()
    {
        // 죽은 적의 EnemyStatusEffect 컴포넌트를 찾기 위해 sender 매개변수 추가
        var enemies = FindObjectsByType<EnemyStatusEffect>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.HasStatusEffect(EnemyStatusEffect.StatusEffectType.Poison) ||
                enemy.HasStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness))
            {
                if (!isBuffActive)
                {
                    StartCoroutine(ApplyDamageBuffCoroutine());
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(ApplyDamageBuffCoroutine());
                }
                break;
            }
        }
    }

    private IEnumerator ApplyDamageBuffCoroutine()
    {
        isBuffActive = true;
        playerStats.SetDamageMultiplier(damageIncreases[skillLevel - 1]);

        yield return new WaitForSeconds(buffDurations[skillLevel - 1]);

        playerStats.SetDamageMultiplier(1f);
        isBuffActive = false;
    }

    private void OnDestroy()
    {
        var enemies = FindObjectsByType<EnemyHealthController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.OnDie -= CheckEnemyStatusOnDeath;
        }
    }
}
