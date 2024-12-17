using UnityEngine;
using System.Collections;

public class B2 : MonoBehaviour
{
    private float damage;
    private float duration;
    private float knockbackForce;
    private float attackInterval;
    private float range;
    private float angle;
    private int skillLevel;
    private SkillData.StatusEffectOnHit statusEffectOnHit;
    private AudioManager audioManager;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        this.damage = skillData.damage * skillLevel;
        this.duration = skillData.duration;
        this.knockbackForce = skillData.knockbackForce;
        this.attackInterval = skillData.attackInterval;
        this.range = skillData.range;
        this.angle = skillData.angle;
        this.skillLevel = skillLevel;
        this.statusEffectOnHit = skillData.statusEffectOnHit;

        // 플레이어 위치에 스킬 배치
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.SetParent(playerTransform);
        transform.position = playerTransform.position;

        // 부채꼴 범위 내 적 감지 및 데미지 처리
        StartCoroutine(DetectAndDamageEnemiesRoutine());
    }

    private bool IsValidAttackDirection(Vector2 directionToTarget)
    {
        // 오른쪽 방향(1, 0)을 기준으로 각도 계산
        float angle = Vector2.SignedAngle(Vector2.right, directionToTarget);

        // 각도를 0~360 범위로 변환
        if (angle < 0)
            angle += 360;

        // 공격 가능한 각도: 345~360도, 0~15도(오른쪽) 및 165~195도(왼쪽)
        bool isValidRight = (angle >= 345 || angle <= 15);
        bool isValidLeft = (angle >= 165 && angle <= 195);

        return isValidRight || isValidLeft;
    }

    private Transform FindNearestValidEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));
        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            Vector2 directionToTarget = (col.transform.position - transform.position).normalized;

            // 사각지대에 있는 적은 무시
            if (!IsValidAttackDirection(directionToTarget))
                continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = col.transform;
            }
        }

        return nearestEnemy;
    }

    private IEnumerator DetectAndDamageEnemiesRoutine()
    {
        float elapsedTime = 0f;
        Transform targetEnemy = FindNearestValidEnemy();

        if (targetEnemy == null)
        {
            Destroy(gameObject);
            yield break;
        }

        audioManager.PlaySFX("S20");
        Vector2 directionToTarget = (targetEnemy.position - transform.position).normalized;
        transform.right = directionToTarget;

        while (elapsedTime < duration)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));

            foreach (Collider2D col in colliders)
            {
                Vector2 dirToEnemy = (col.transform.position - transform.position).normalized;
                float angleToTarget = Vector2.Angle(transform.right, dirToEnemy);

                if (angleToTarget <= angle / 2)
                {
                    EnemyHealthController enemyHealth = col.GetComponent<EnemyHealthController>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage, -dirToEnemy * knockbackForce);

                        if (statusEffectOnHit != SkillData.StatusEffectOnHit.None)
                        {
                            EnemyStatusEffect statusEffect = enemyHealth.GetComponent<EnemyStatusEffect>();
                            if (statusEffect != null)
                            {
                                statusEffect.ApplyStatusEffect(ConvertToEnemyStatusEffectType(statusEffectOnHit), 2f);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(attackInterval);
            elapsedTime += attackInterval;
        }

        Destroy(gameObject);
    }

    private EnemyStatusEffect.StatusEffectType ConvertToEnemyStatusEffectType(SkillData.StatusEffectOnHit effect)
    {
        switch (effect)
        {
            case SkillData.StatusEffectOnHit.Slow:
                return EnemyStatusEffect.StatusEffectType.Slow;
            case SkillData.StatusEffectOnHit.Bleed:
                return EnemyStatusEffect.StatusEffectType.Bleed;
            case SkillData.StatusEffectOnHit.Poison:
                return EnemyStatusEffect.StatusEffectType.Poison;
            default:
                return EnemyStatusEffect.StatusEffectType.None;
        }
    }

    // =================================

    private void OnDrawGizmos()
    {
        // 기존 공격 범위 기즈모 (빨간색)
        Gizmos.color = Color.red;
        Vector3 direction = transform.right;
        Vector3 leftDirection = Quaternion.Euler(0, 0, angle / 2) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -angle / 2) * direction;

        Gizmos.DrawLine(transform.position, transform.position + leftDirection * range);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * range);
        DrawArc();

        // 공격 가능한 각도 기즈모 (파란색)
        Gizmos.color = Color.blue;

        // 오른쪽 방향 (345~15도)
        Vector3 rightStart = Quaternion.Euler(0, 0, -15) * Vector3.right;
        Vector3 rightEnd = Quaternion.Euler(0, 0, 15) * Vector3.right;
        DrawArcSegment(-15f, 15f, Color.blue);

        // 왼쪽 방향 (165~195도)
        Vector3 leftStart = Quaternion.Euler(0, 0, 165) * Vector3.right;
        Vector3 leftEnd = Quaternion.Euler(0, 0, 195) * Vector3.right;
        DrawArcSegment(165f, 195f, Color.blue);
    }

    private void DrawArcSegment(float startAngle, float endAngle, Color color)
    {
        float arcAngleStep = 1f;
        Vector3 center = transform.position;

        // Arc의 시작점과 끝점
        Vector3 startPoint = center + Quaternion.Euler(0, 0, startAngle) * Vector3.right * range;
        Vector3 endPoint = center + Quaternion.Euler(0, 0, endAngle) * Vector3.right * range;

        // 플레이어 위치에서 Arc의 시작점과 끝점으로 선 그리기
        Gizmos.DrawLine(center, startPoint);
        Gizmos.DrawLine(center, endPoint);

        // Arc 그리기
        for (float i = startAngle; i <= endAngle; i += arcAngleStep)
        {
            float start = i;
            float end = i + arcAngleStep;

            Vector3 startPos = center + Quaternion.Euler(0, 0, start) * Vector3.right * range;
            Vector3 endPos = center + Quaternion.Euler(0, 0, end) * Vector3.right * range;

            Gizmos.DrawLine(startPos, endPos);
        }
    }

    private void DrawArc()
    {
        Vector3 center = transform.position;
        float arcAngleStep = 1f;

        for (float i = -angle / 2; i <= angle / 2; i += arcAngleStep)
        {
            float startAngle = i;
            float endAngle = i + arcAngleStep;

            Vector3 start = center + Quaternion.Euler(0, 0, startAngle) * transform.right * range;
            Vector3 end = center + Quaternion.Euler(0, 0, endAngle) * transform.right * range;

            Gizmos.DrawLine(start, end);
        }
    }
}
