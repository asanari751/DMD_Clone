using UnityEngine;
using System.Collections;

public class S6 : MonoBehaviour
{
    private SkillData skillData;
    private int skillLevel;
    private float moveSpeedBonus;
    private float footprintInterval = 2f;
    private float footprintDuration = 3f;
    private float weaknessDuration = 8f;
    private bool isActive = true;
    private PlayerStats playerStats;

    private readonly float[] moveSpeedBonusValues = { 1.1f, 1.15f, 1.2f };

    public void Initialize(SkillData data, int level)
    {
        skillData = data;
        skillLevel = level;
        playerStats = PlayerStats.Instance;
        moveSpeedBonus = moveSpeedBonusValues[skillLevel - 1];

        // 이동속도 증가 적용
        float currentMoveSpeed = playerStats.moveSpeed;
        playerStats.SetMoveSpeed(currentMoveSpeed * moveSpeedBonus);

        StartCoroutine(CreateFootprints());
    }

    private IEnumerator CreateFootprints()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        while (isActive)
        {
            GameObject footprint = Instantiate(skillData.skillPrefab, playerTransform.position, Quaternion.identity);
            S6Footprint footprintComponent = footprint.AddComponent<S6Footprint>();
            footprintComponent.Initialize(weaknessDuration, footprintDuration, skillData);

            yield return new WaitForSeconds(footprintInterval);  // 선언된 변수 사용
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            // 이동속도 원래대로 복구
            float currentMoveSpeed = playerStats.moveSpeed;
            playerStats.SetMoveSpeed(currentMoveSpeed / moveSpeedBonus);
        }
        isActive = false;
    }
}

public class S6Footprint : MonoBehaviour
{
    private SkillData skillData;
    private float weaknessDuration;
    private CircleCollider2D circleCollider;

    public void Initialize(float weaknessDuration, float duration, SkillData skillData)
    {
        this.weaknessDuration = weaknessDuration;

        // 콜라이더 설정
        circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = 3f;

        // 발자국 프리팹의 애니메이션 실행
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.skillName);
        }

        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStatusEffect statusEffect = collision.GetComponent<EnemyStatusEffect>();
            if (statusEffect != null)
            {
                statusEffect.ApplyStatusEffect(EnemyStatusEffect.StatusEffectType.Weakness, weaknessDuration);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (circleCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
        }
    }
}
