using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyBoss : BasicEnemy
{
    [Header("Charge Attack Settings")]
    [SerializeField] private float chargeReadyDuration = 1.5f;
    [SerializeField] private float chargeDuration = 1.0f;
    [SerializeField] private float chargeDistance = 300f;
    [SerializeField] private float chargeAttackRange = 300f;
    [SerializeField] private float chargeDamage = 20f;

    [Header("Charge Attack Visual")]
    [SerializeField] private Transform prefabSpawnPoint;
    [SerializeField] private GameObject chargeAttackAreaPrefab;
    private GameObject chargeAttackAreaInstance;
    private SpriteRenderer chargeAttackAreaRenderer;

    private BossAnimationController bossAnimController;
    private Vector2 chargeDirection;
    private bool isCharging = false;

    public event System.Action OnBossEnemyDeath;
    private EnemyHealthBoss enemyHealthBoss;
    private EnemyHealthController enemyHealthController;

    private float CustomEaseCharge(float x)
    {
        float prepareTime = 0.2f;
        float chargeTime = 0.4f;

        if (x <= prepareTime)
        {
            // 10% 이동
            return (0.1f * x) / prepareTime;
        }
        else if (x <= prepareTime + chargeTime)
        {
            // 90% 이동
            float t = (x - prepareTime) / chargeTime;
            return 0.1f + (0.9f * t);
        }
        else
        {
            // 이동 완료
            return 1.0f;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        enemyHealthController = GetComponent<EnemyHealthController>();
        enemyHealthBoss = GetComponent<EnemyHealthBoss>();
        bossAnimController = GetComponent<BossAnimationController>();

        if (enemyHealthController != null)
        {
            enemyHealthController.OnDie += HandleOnDie;
        }
    }

    private void OnDestroy()
    {
        if (enemyHealthController != null)
        {
            enemyHealthController.OnDie -= HandleOnDie;
        }

        if (chargeAttackAreaInstance != null)
        {
            Destroy(chargeAttackAreaInstance);
        }
    }

    private void HandleOnDie()
    {
        if (enemyHealthBoss != null)
        {
            enemyHealthBoss.DestroyHealthBar();
        }

        OnBossEnemyDeath?.Invoke();
    }

    public override void CheckAttack()
    {
        if (GetComponent<EnemyHealthController>().IsDead() || playerTransform == null) return;

        PlayerHealthUI playerHealth = playerTransform.GetComponent<PlayerHealthUI>();
        if (playerHealth != null && playerHealth.IsDead()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= chargeAttackRange && !isCharging)
        {
            StartChargeAttack();
        }
    }

    public void StartChargeAttack()
    {
        if (!isCharging)
            StartCoroutine(ChargeAttackCoroutine());
    }

    private IEnumerator ChargeAttackCoroutine()
    {
        isCharging = true;
        SetCanMove(false);

        // 돌격 준비
        ShowChargeAttackArea();
        bossAnimController.PlayChargeReadyAnimation();
        chargeDirection = (playerTransform.position - transform.position).normalized;
        yield return new WaitForSeconds(chargeReadyDuration);

        // 여기서 공격 범위 프리팹 비활성화
        if (chargeAttackAreaInstance != null)
        {
            chargeAttackAreaInstance.SetActive(false);
        }

        // 돌격
        bossAnimController.PlayChargeAnimation();
        float elapsedTime = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (chargeDirection * chargeDistance);

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeDuration;
            // EaseInBack 적용
            float easedT = CustomEaseCharge(t);
            transform.position = Vector2.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        // 돌격 후 1초간 정지
        bossAnimController.StopCharging();
        yield return new WaitForSeconds(1f);

        isCharging = false;
        SetCanMove(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Player"))
        {
            var playerHealth = collision.gameObject.GetComponent<PlayerHealthUI>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                playerHealth.TakeDamageWithKnockback(chargeDamage, knockbackDir, 7f); // 넉백 힘은 조절 가능
            }
        }
    }

    private void ShowChargeAttackArea()
    {
        if (chargeAttackAreaPrefab != null)
        {
            Vector3 spawnPosition = prefabSpawnPoint != null ? prefabSpawnPoint.position : transform.position;

            if (chargeAttackAreaInstance == null)
            {
                chargeAttackAreaInstance = Instantiate(chargeAttackAreaPrefab, spawnPosition, Quaternion.identity, transform);
                chargeAttackAreaRenderer = chargeAttackAreaInstance.GetComponent<SpriteRenderer>();
            }
            else
            {
                chargeAttackAreaInstance.transform.position = spawnPosition;
                chargeAttackAreaInstance.SetActive(true);
            }

            // 플레이어 방향으로 회전
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            chargeAttackAreaInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 크기 설정
            chargeAttackAreaInstance.transform.localScale = new Vector3(chargeDistance / 10, chargeDistance / 4, 1);

            // 색상 변화 애니메이션
            chargeAttackAreaRenderer.color = new Color(1f, 0f, 0f, 0.5f); // 시작 색상
            chargeAttackAreaRenderer.DOColor(new Color(1f, 0f, 0f, 0.8f), chargeReadyDuration);
        }
    }

    // =============

    private void OnDrawGizmosSelected() // 기즈모 - 개발 완료시 제거
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeAttackRange);
    }
}