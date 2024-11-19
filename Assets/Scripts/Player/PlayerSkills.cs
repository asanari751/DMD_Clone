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
        // 패시브 스킬은 코루틴 실행하지 않음
        if (skill.skillData.isPassive)
        {
            UseSkill(skill);
            yield break;
        }

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

            case "가시의 저주":
                UseB5(skill);
                break;

            case "피의 폭발":
                UseB6(skill);
                break;

            case "피의 목욕":
                UseB7(skill);
                break;

            case "영원한 젊음":
                UseB8(skill);
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

    private void UseB2(Skill skill)
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B2 b2 = skillEffect.AddComponent<B2>();
        b2.Initialize(skill.skillData, skill.skillLevel);

        // Animator animator = skillEffect.GetComponent<Animator>();
        // if (animator != null)
        // {
        //     animator.Play(skill.skillData.skillName);
        // }
    }

    private void UseB3(Skill skill)
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B3 b3 = skillEffect.AddComponent<B3>();
        b3.Initialize(skill.skillData, skill.skillLevel);

        // Animator animator = skillEffect.GetComponent<Animator>();
        // if (animator != null)
        // {
        //     animator.Play(skill.skillData.skillName);
        // }
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

    private void UseB5(Skill skill)
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B5 b5 = skillEffect.AddComponent<B5>();
        b5.Initialize(skill.skillData, skill.skillLevel);

        // Animator animator = skillEffect.GetComponent<Animator>();
        // if (animator != null)
        // {
        //     animator.Play(skill.skillData.skillName);
        // }
    }

    private void UseB6(Skill skill)
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B6 b6 = skillEffect.AddComponent<B6>();
        b6.Initialize(skill.skillData, skill.skillLevel);

        // Animator animator = skillEffect.GetComponent<Animator>();
        // if (animator != null)
        // {
        //     animator.Play(skill.skillData.skillName);
        // }
    }

    private void UseB7(Skill skill)
    {
        GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
        B7 b7 = skillEffect.AddComponent<B7>();
        b7.Initialize(skill.skillData, skill.skillLevel);
    }

    private void UseB8(Skill skill)
    {
        // 이미 존재하는 B8 컴포넌트 확인
        B8 existingB8 = FindAnyObjectByType<B8>();

        // 없을 때만 새로 생성
        if (existingB8 == null)
        {
            GameObject skillEffect = Instantiate(skill.skillData.skillPrefab);
            B8 b8 = skillEffect.AddComponent<B8>();
            b8.Initialize(skill.skillData, skill.skillLevel);
        }
    }
}