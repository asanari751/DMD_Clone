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
    public float attackInterval;
}

public class PlayerSkills : MonoBehaviour
{

    public static PlayerSkills Instance { get; private set; }

    [SerializeField] private GameObject skill1EffectPrefab;
    [SerializeField] private GameObject skill2EffectPrefab;

    [SerializeField]
    private SkillStats skill1Stats = new SkillStats
    {
        radius = 50f,
        knockbackForce = 50f,
        damage = 2f,
        duration = 5f,
        cooldown = 8f,
        attackInterval = 1f  // 기본값 1초로 설정
    };

    [SerializeField]
    private SkillStats skill2Stats = new SkillStats
    {
        radius = 50f,
        knockbackForce = 50f,
        damage = 1f,
        duration = 5f,
        cooldown = 8f,
        attackInterval = 1f  // 기본값 1초로 설정
    };

    [SerializeField] private float skill2SpawnRadius = 100f;

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private bool isSkill1Ready = true;
    private bool isSkill2Ready = true;

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
        StartCoroutine(AutoCastSkill2());
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

        float levelMultiplier = Mathf.Pow(2, GetSkillLevel("Iron Maiden") - 1);
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

    private IEnumerator AutoCastSkill2()
    {
        while (true)
        {
            if (skillLevels.ContainsKey("아이언 메이든") && isSkill2Ready)
            {
                UseSkill2();
                isSkill2Ready = false;
                yield return new WaitForSeconds(skill2Stats.cooldown);
                isSkill2Ready = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UseSkill2()
    {
        // 랜덤 위치 계산 (플레이어 주변 100픽셀 이내)
        Vector2 randomOffset = Random.insideUnitCircle * skill2SpawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // 아이언 메이든 이펙트 생성
        GameObject ironMaiden = Instantiate(skill2EffectPrefab, spawnPosition, Quaternion.identity);


        // SpriteRenderer sorting order 설정
        SpriteRenderer spriteRenderer = ironMaiden.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-spawnPosition.y * 100);
        }

        // 애니메이터 실행
        Animator animator = ironMaiden.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("Iron Maiden");
        }

        float currentSkillLevel = GetSkillLevel("아이언 메이든");
        float finalDamage = skill2Stats.damage;

        // 데미지 영역 설정
        CircleCollider2D collider = ironMaiden.AddComponent<CircleCollider2D>();
        collider.radius = skill2Stats.radius;
        collider.isTrigger = true;

        // 주기적으로 적을 끌어당기고 데미지를 주는 컴포넌트 추가
        IronMaiden maiden = ironMaiden.AddComponent<IronMaiden>();
        maiden.Initialize(finalDamage, skill2Stats.knockbackForce, skill2Stats.duration, skill2Stats.attackInterval);

        // 지속시간 후 제거
        Destroy(ironMaiden, skill2Stats.duration);
    }

    private IEnumerator EnableColliderAfterDelay(Collider2D collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (collider != null)
            collider.enabled = true;
    }
}
