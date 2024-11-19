using UnityEngine;

public class SkillData : ScriptableObject
{
    public enum SkillRangeType { Single, Aoe, Cone, Line, Self }
    public enum EffectOnHit { None, Fear, Bleed, Slow, Weakness, Poison, Stun }

    // 기본
    public SkillRangeType skillRangeType;
    public string skillName;
    public Sprite skillIcon;
    public int skillLevel;
    [TextArea]
    public string skillDescription;
    public GameObject skillPrefab;
    public float damage;
    public float duration;
    public float cooldown;
    public bool isPassive;

    // 공격
    [SerializeField] public float attackInterval;
    [SerializeField] public EffectOnHit effectOnHit;
    [SerializeField] public float radius;    // Aoe
    [SerializeField] public float range;     // Line
    [SerializeField] public float angle;     // Cone
    [SerializeField] public int maxTargets;  // Single
    [SerializeField] public float knockbackForce;

    // 패시브
    [SerializeField] public float procChance; // 발동 확률
    [SerializeField] public float effectValue;
}
