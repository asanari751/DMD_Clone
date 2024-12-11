using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public enum GodOwner
    {
        Bathory = 0,
        Strigoi = 1,
        Vlad = 2,
    }

    // 스킬의 범위 유형 정의
    public enum SkillRangeType { Single, Circle, Cone, Line, Self }

    // 적중 시 발생하는 상태이상 효과
    public enum StatusEffectOnHit { None, Fear, Bleed, Slow, Weakness, Poison, Stun }

    // ============================
    // UI 관련 정보
    // ============================

    [Header("UI 관련 정보")]

    [Tooltip("스킬 이름")]
    public string skillName;

    [Tooltip("스킬 아이콘")]
    public Sprite skillIcon;

    [Tooltip("스킬 레벨")]
    public int skillLevel;

    [Tooltip("스킬 설명")]

    [TextArea]
    public string skillDescription;
    public GodOwner godOwner;

    // ============================
    // 스킬의 기본 정보
    // ============================
    [Header("스킬 기본 정보")]
    [Tooltip("스킬의 범위 유형 ( Single, Circle, Cone, Line, Self )")]
    public SkillRangeType skillRangeType;

    [Tooltip("스킬 프리팹")]
    public GameObject skillPrefab;

    [Tooltip("스킬 데미지")]
    public float damage;

    [Tooltip("스킬 효과의 지속 시간")]
    public float duration;

    [Tooltip("스킬의 재사용 대기시간")]
    public float cooldown;

    [Tooltip("패시브 여부")]
    public bool isPassive;

    // ============================
    // 공격 관련 데이터
    // ============================

    [Header("공격 관련 데이터")]

    [Tooltip("공격 간격")]
    [SerializeField] public float attackInterval;

    [Tooltip("적중 시 상태이상 효과")]
    [SerializeField] public StatusEffectOnHit statusEffectOnHit;

    [Tooltip("Circle - 스킬 반경")]
    [SerializeField] public float radius;

    [Tooltip("Line - 사거리")]
    [SerializeField] public float range;

    [Tooltip("Cone - 각도")]
    [SerializeField] public float angle;

    [Tooltip("최대 타겟 수")]
    [SerializeField] public int maxTargets;

    [Tooltip("밀어내는 힘의 크기")]
    [SerializeField] public float knockbackForce;

    // ============================
    // 패시브 관련 데이터
    // ============================

    [Header("패시브 관련 데이터")]

    [Tooltip("패시브의 발동 확률 ( 0 ~ 100 )")]
    [SerializeField] public float procChance;

    [Tooltip("패시브의 효과")]
    [SerializeField] public float effectValue;
}
