using UnityEngine;

public class V1 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float damage;
    private float range = 20f;  // 검기 사출 거리
    private float cooldown;
    private static int attackCounter = 0;
    private static bool isSkillReady = false;
    private AudioManager audioManager;

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        cooldown = skillData.cooldown;
        damage = skillData.damage + (skillLevel * 25);
        audioManager = FindAnyObjectByType<AudioManager>();

        // 플레이어의 기본 공격 이벤트 구독
        PlayerStateManager playerStateManager = FindAnyObjectByType<PlayerStateManager>();
        if (playerStateManager != null)
        {
            // 기본 공격 카운터 이벤트 추가 필요
        }
    }

    public static void IncrementAttackCounter()
    {
        attackCounter++;
        if (attackCounter >= 4)
        {
            isSkillReady = true;
            attackCounter = 0;
        }
    }

    public static bool IsSkillReady()
    {
        return isSkillReady;
    }

    public static void ResetSkillReady()
    {
        isSkillReady = false;
    }

    private void Start()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = playerTransform.position;

        // 가장 가까운 적 방향으로 발사
        Vector2 direction = FindNearestEnemyDirection();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 프리팹 이동 추가
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = direction * range;

        // 애니메이션 실행
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        audioManager.PlaySFX("S11");
        Destroy(gameObject, 1f);
    }

    private Vector2 FindNearestEnemyDirection()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Vector2 nearestDirection = Vector2.right;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestDirection = (enemy.transform.position - transform.position).normalized;
            }
        }

        return nearestDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthController enemyHealth = collision.GetComponent<EnemyHealthController>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                enemyHealth.TakeDamage(damage, -knockbackDirection * skillData.knockbackForce);
            }
        }
    }
}
