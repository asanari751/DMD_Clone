using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class SkillStats
{
    public float radius;
    public float knockbackForce;
    public float damage;
    public float duration;
    public float cooldown;
}

public class PlayerSkills : MonoBehaviour
{

    public static PlayerSkills Instance { get; private set; }

    [SerializeField] private GameObject skill1EffectPrefab;
    [SerializeField]
    private SkillStats skill1Stats = new SkillStats
    {
        radius = 250f,
        knockbackForce = 100f,
        damage = 1f,
        duration = 2f,
        cooldown = 5f
    };

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private bool isSkill1Ready = true;

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
        StartCoroutine(AutoCastSkill1());
    }

    public void AddOrUpgradeSkill(string skillName)
    {
        if (skillLevels.ContainsKey(skillName))
        {
            skillLevels[skillName]++;
        }
        else
        {
            skillLevels[skillName] = 2;
        }
    }

    public int GetSkillLevel(string skillName)
    {
        return skillLevels.TryGetValue(skillName, out int level) ? level : 0;
    }

    private IEnumerator AutoCastSkill1()
    {
        while (true)
        {
            if (skillLevels.ContainsKey("Skill 1") && isSkill1Ready)
            {
                UseSkill1();
                isSkill1Ready = false;
                yield return new WaitForSeconds(skill1Stats.cooldown);
                isSkill1Ready = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UseSkill1()
    {
        // 이펙트
        GameObject skillEffect = Instantiate(skill1EffectPrefab, transform.position, Quaternion.identity);
        skillEffect.transform.SetParent(transform);

        Animator animator = skillEffect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("Test");
        }

        float levelMultiplier = Mathf.Pow(2, GetSkillLevel("Skill 1") - 1);
        float finalDamage = skill1Stats.damage * levelMultiplier;

        CircleCollider2D collider = skillEffect.AddComponent<CircleCollider2D>();
        collider.radius = skill1Stats.radius;
        collider.isTrigger = true;
        collider.enabled = false;

        SkillDamageArea damageArea = skillEffect.AddComponent<SkillDamageArea>();
        damageArea.Initialize(finalDamage, skill1Stats.knockbackForce);

        StartCoroutine(EnableColliderAfterDelay(collider, 0.15f));

        // 스킬 이펙트 제거
        Destroy(skillEffect, skill1Stats.duration);
    }

    private IEnumerator EnableColliderAfterDelay(Collider2D collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (collider != null)
            collider.enabled = true;
    }
}
