using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skills/Skill Data", order = 1)]
public class SkillData : ScriptableObject
{
    public enum SkillRangeType { Single, Aoe, Cone, Line, Self }
    public enum EffectOnHit { None, Fear, Bleed, Slow, Weakness, Poison, Stun }

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
    public float attackInterval;
    public bool isPassive;
    public EffectOnHit effectOnHit;

    // 스킬 타입별 속성
    [SerializeField] public float radius;    // Aoe용
    [SerializeField] public float range;     // Line용
    [SerializeField] public float angle;     // Cone용
    [SerializeField] public int maxTargets;  // Single용
    [SerializeField] public float knockbackForce; // 넉백 효과용
}
