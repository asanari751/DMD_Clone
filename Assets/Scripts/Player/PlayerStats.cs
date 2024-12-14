using UnityEngine;

[DefaultExecutionOrder(-100)]
[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStateManager playerStateManager;

    // 1. Attack
    public float damageMultiplier { get; private set; }
    public float criticalDamage { get; private set; }
    public float criticalChance { get; private set; }
    public float range { get; private set; }

    // 2. Defense
    public float maxHealth { get; private set; }
    public float armor { get; private set; }
    public float reduction { get; private set; }
    public float dodge { get; private set; }

    // 3. Default
    public float defaultDamage { get; private set; }
    public float defaultAttackSpeed { get; private set; }
    public float defaultRange { get; private set; }
    public float defaultCriticalChance { get; private set; }

    // 3-1. Unique Default ( Unvisible )
    public float projectileSpeed { get; private set; }
    public int penetrateCount { get; private set; }

    // 4. Utility
    public float moveSpeed { get; private set; }
    public float lootRange { get; private set; }
    public float dashRange { get; private set; }
    public float dashCount { get; private set; }

    public event System.Action<CharacterInfo.PlayerStats> OnStatsUpdated;

    // ===============================================

    private void Start()
    {
        playerStateManager = FindAnyObjectByType<PlayerStateManager>();
    }

    public void SetDamageMultiplier(float value)
    {
        damageMultiplier = value;
        if (playerStateManager != null)
        {
            playerStateManager.UpdateStats();
        }
    }
    
    public void SetCriticalDamage(float value) => criticalDamage = value;
    public void SetCriticalChance(float value)
    {
        criticalChance = value;
        if (playerStateManager != null)
        {
            playerStateManager.UpdateStats();
        }
    }

    public void SetRange(float value) => range = value;

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        if (playerStateManager != null)
        {
            playerStateManager.UpdateStats();
        }
    }

    public void SetArmor(float value) => armor = value;
    public void SetReduction(float value) => reduction = value;
    public void SetDodge(float value) => dodge = value;

    public void SetDefaultDamage(float value) => defaultDamage = value;
    public void SetDefaultAttackSpeed(float value) => defaultAttackSpeed = value;
    public void SetDefaultRange(float value) => defaultRange = value;
    public void SetDefaultCriticalChance(float value) => defaultCriticalChance = value;

    public void SetMoveSpeed(float value) => moveSpeed = value;
    public void SetLootRange(float value) => lootRange = value;
    public void SetDashRange(float value) => dashRange = value;
    public void SetDashCount(float value) => dashCount = value;

    private void Awake()
    {
        SetDamageMultiplier(1f);
        SetCriticalDamage(3f);
        SetCriticalChance(0.01f);
        SetRange(1f);

        SetMaxHealth(25f);
        SetArmor(0f);
        SetReduction(0f);
        SetDodge(0f);

        SetDefaultDamage(45f);
        SetDefaultAttackSpeed(0.5f);
        SetDefaultRange(5f);
        SetDefaultCriticalChance(0.1f);

        SetMoveSpeed(5f);
        SetLootRange(2.5f);
        SetDashRange(5f);
        SetDashCount(2f);
    }

    private CharacterInfo.PlayerStats GetCurrentStats()
    {
        return new CharacterInfo.PlayerStats
        {
            // Attack
            damageMultiplier = this.damageMultiplier,
            criticalDamage = this.criticalDamage,
            criticalChance = this.criticalChance,
            range = this.range,

            // Defense
            maxHealth = this.maxHealth,
            armor = this.armor,
            reduction = this.reduction,
            dodge = this.dodge,

            // Default
            defaultDamage = this.defaultDamage,
            defaultAttackSpeed = this.defaultAttackSpeed,
            defaultRange = this.defaultRange,
            defaultCriticalChance = this.defaultCriticalChance,

            // Utility
            moveSpeed = this.moveSpeed,
            lootRange = this.lootRange,
            dashRange = this.dashRange,
            dashCount = this.dashCount
        };
    }
}
