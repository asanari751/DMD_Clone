using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class V2 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float damage;
    private float duration;
    private float stunDuration = 1f;
    private float detectionRadius;
    private int maxTargets;
    private AudioManager audioManager;

    private readonly float[] damageValues = { 120f, 150f, 210f };
    private readonly float[] durationValues = { 10f, 9f, 8f };
    private readonly float[] radiusValues = { 6f, 7f, 8f };
    private readonly int[] targetValues = { 3, 5, 7 };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;

        int index = skillLevel - 1;
        damage = damageValues[index];
        duration = durationValues[index];
        detectionRadius = radiusValues[index];
        maxTargets = targetValues[index];
        audioManager = FindAnyObjectByType<AudioManager>();

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // 메인 오브젝트의 애니메이션 실행
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        audioManager.PlaySFX("S12");
        SpawnMultipleSkillEffects();
        StartCoroutine(DestroyAfterDuration());
    }

    private void SpawnMultipleSkillEffects()
    {
        List<Vector3> targetPositions = FindMultipleEnemyPositions();

        foreach (Vector3 position in targetPositions)
        {
            GameObject skillEffect = Instantiate(skillData.skillPrefab, position, Quaternion.identity);
            skillEffect.transform.SetParent(transform);

            // 콜라이더 추가 및 크기 설정
            CircleCollider2D collider = skillEffect.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1f;  // 콜라이더 크기를 1로 설정

            // 데미지 처리 컴포넌트 추가
            V2Damage damageD = skillEffect.AddComponent<V2Damage>();
            damageD.Initialize(damage, stunDuration);

            // 애니메이션 실행
            Animator animator = skillEffect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(skillData.skillName);
            }
        }
    }


    private List<Vector3> FindMultipleEnemyPositions()
    {
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        Debug.Log($"플레이어 위치: {playerPosition}");

        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, detectionRadius);
        Debug.Log($"감지된 콜라이더 수: {colliders.Length}");

        List<Vector3> enemyPositions = new List<Vector3>();

        var enemyColliders = colliders
            .Where(c => c.CompareTag("Enemy"))
            .OrderBy(c => Vector2.Distance(playerPosition, c.transform.position))
            .Take(maxTargets);

        foreach (var collider in enemyColliders)
        {
            Debug.Log($"찾은 적: {collider.gameObject.name}, 위치: {collider.transform.position}");
            enemyPositions.Add(collider.transform.position);
        }

        Debug.Log($"최종 타겟 수: {enemyPositions.Count}");
        return enemyPositions;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, Vector2.zero);
            }

            EnemyStatusEffect statusEffect = other.GetComponent<EnemyStatusEffect>();
            if (statusEffect != null)
            {
                // statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Stun, stunDuration);
            }
        }
    }

    private IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(1.2f);
        Destroy(gameObject);
    }
}
