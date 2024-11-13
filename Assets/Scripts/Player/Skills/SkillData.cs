using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skills/Skill Data", order = 1)]
public class SkillData : ScriptableObject
{
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
    
    
    
}