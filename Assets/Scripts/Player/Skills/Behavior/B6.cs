using System.Runtime.CompilerServices;
using UnityEngine;

public class B6 : MonoBehaviour
{
    private float damage;
    private float radius;
    private float knockbackForce;
    private SkillData.EffectOnHit effectOnHit;

    public void Initialize(SkillData skillData, int skillLevel)
    {
        damage = skillData.damage * skillLevel;
        radius = skillData.radius;
        effectOnHit = skillData.effectOnHit;
        knockbackForce = skillData.knockbackForce;

        // 스킬 범위 설정
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = radius;
        collider.isTrigger = true;

        Vector2 targetPos = FindMostCrowdedPosition(radius);
        transform.position = targetPos;

        DealExplosionDamage();
    }

    private void DealExplosionDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (Collider2D enemy in enemies)
        {
            EnemyHealthController enemyHealth = enemy.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                // 폭발 중심점으로부터의 방향 계산
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;

                // 데미지와 넉백 적용
                enemyHealth.TakeDamage(damage, -knockbackDirection * knockbackForce);
            }
        }

        Destroy(gameObject, 0.5f);
    }

    private bool IsPositionVisible(Vector2 position)
    {
        Camera mainCamera = Camera.main;
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    private Vector2 FindMostCrowdedPosition(float radius)
    {
        // 플레이어의 현재 위치를 기준으로 계산
        Vector2 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

        // 플레이어 주변의 적 탐지
        Collider2D[] allEnemies = Physics2D.OverlapCircleAll(playerPosition, radius * 2f, LayerMask.GetMask("Enemy"));
        if (allEnemies.Length == 0) return playerPosition;

        Vector2 bestPosition = playerPosition;
        int maxEnemyCount = 0;

        foreach (Collider2D enemy in allEnemies)
        {
            Vector2 checkPosition = enemy.transform.position;

            if (!IsPositionVisible(checkPosition))
                continue;

            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(checkPosition, radius, LayerMask.GetMask("Enemy"));

            if (nearbyEnemies.Length > maxEnemyCount)
            {
                maxEnemyCount = nearbyEnemies.Length;
                bestPosition = checkPosition;
            }
        }

        return bestPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawAoeRange();
    }

    private void DrawAoeRange()
    {
        float arcAngleStep = 10f;
        Vector3 center = transform.position;

        for (float angle = 0; angle < 360; angle += arcAngleStep)
        {
            float rad = angle * Mathf.Deg2Rad;
            float nextRad = (angle + arcAngleStep) * Mathf.Deg2Rad;

            Vector3 start = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector3 end = center + new Vector3(Mathf.Cos(nextRad), Mathf.Sin(nextRad)) * radius;

            Gizmos.DrawLine(start, end);
        }
    }

}

