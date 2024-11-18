using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Skill
{
    public SkillData skillData;
    public bool isReady = true;
    public int skillLevel = 1;

    public Skill(SkillData data)
    {
        skillData = data;
    }
}

public class PlayerSkills : MonoBehaviour
{

    public static PlayerSkills Instance { get; private set; }

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    public List<Skill> activeSkills = new List<Skill>();
    private EnemyHealthController enemyHealthController;
    private const int defaultSkillLevel = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var skill in activeSkills)
        {
            StartCoroutine(AutoCastSkill(skill));
        }
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        var existingSkill = activeSkills.Find(s => s.skillData.skillName == skillData.skillName);
        if (existingSkill != null)
        {
            existingSkill.skillLevel++;
            skillLevels[skillData.skillName] = existingSkill.skillLevel;
        }
        else
        {
            Skill newSkill = new Skill(skillData);
            activeSkills.Add(newSkill);
            skillLevels[skillData.skillName] = 1;
            StartCoroutine(AutoCastSkill(newSkill));
        }
    }

    public int GetSkillLevel(string skillName)
    {
        var skill = activeSkills.Find(s => s.skillData.skillName == skillName);
        return skill != null ? skill.skillLevel : defaultSkillLevel;
    }

    private IEnumerator AutoCastSkill(Skill skill)
    {
        while (true)
        {
            if (skill.isReady)
            {
                UseSkill(skill);
                skill.isReady = false;
                yield return new WaitForSeconds(skill.skillData.cooldown);
                skill.isReady = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UseSkill(Skill skill)
    {
        switch (skill.skillData.skillName)
        {
            case "가시의 감옥":
                UseB1(skill);
                break;

            case "가시 채찍":
                UseB2(skill);
                break;

            case "피의 화살":
                UseB3(skill);
                break;

            case "아이언 메이든":
                UseB4(skill);
                break;

            //

            default:
                Debug.LogWarning($"알 수 없는 스킬: {skill.skillData.skillName}");
                break;
        }
    }

    // 스킬 실행단

    private void UseB1(Skill skill) // B1
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B1 b1 = skillEffect.AddComponent<B1>();
        b1.Initialize(skill.skillData, skill.skillLevel);

        Animator animator = skillEffect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skill.skillData.skillName);
        }
    }

    private void UseB2(Skill skill) // B2
    {
        // 
    }

    private void UseB3(Skill skill) // B3
    {
        //
    }

    private void UseB4(Skill skill) // B4
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B4 b4 = skillEffect.AddComponent<B4>();
        b4.Initialize(skill.skillData, skill.skillLevel);

        Animator animator = skillEffect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skill.skillData.skillName);
        }
    }
}