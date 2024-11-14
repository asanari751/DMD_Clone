using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skills/Skill Data", order = 1)]
public class SkillData : ScriptableObject
{
    public enum SkillRangeType { Single, Aoe, Cone, Line, Self } // 스킬 범위 유형 정의
    public enum EffectOnHit { None, Fear, Bleed, Slow, Weakness, Poison, Stun } // 상태이상 효과 정의

    public SkillRangeType skillRangeType; // 스킬의 범위 유형
    public string skillName;
    public Sprite skillIcon;
    public int skillLevel;
    [TextArea]
    public string skillDescription;
    public GameObject skillPrefab;
    public float radius;
    public float knockbackForce;
    public float damage;
    public float duration;
    public float cooldown;
    public float attackInterval;
    public bool isPassive; // 패시브 스킬 여부
    public EffectOnHit effectOnHit; // 스킬로 인한 상태이상
    public float castTime; // 스킬의 시전시간
    public bool isAoe; // 범위 공격 여부
    public int maxTargets; // 최대 공격할 수 있는 타겟 수
}