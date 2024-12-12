using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
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
    public float dashCooldown { get; private set; }
    public float dashCount { get; private set; }

    // ===============================================

    public void SetDamageMultiplier(float value) => damageMultiplier = value;
    public void SetCriticalDamage(float value) => criticalDamage = value;
    public void SetCriticalChance(float value) => criticalChance = value;
    public void SetRange(float value) => range = value;

    public void SetMaxHealth(float value) => maxHealth = value;
    public void SetArmor(float value) => armor = value;
    public void SetReduction(float value) => reduction = value;
    public void SetDodge(float value) => dodge = value;

    public void SetDefaultDamage(float value) => defaultDamage = value;
    public void SetDefaultAttackSpeed(float value) => defaultAttackSpeed = value;
    public void SetDefaultRange(float value) => defaultRange = value;
    public void SetDefaultCriticalChance(float value) => defaultCriticalChance = value;

    public void SetMoveSpeed(float value) => moveSpeed = value;
    public void SetLootRange(float value) => lootRange = value;
    public void SetDashCooldown(float value) => dashCooldown = value;
    public void SetDashCount(float value) => dashCount = value;
}
