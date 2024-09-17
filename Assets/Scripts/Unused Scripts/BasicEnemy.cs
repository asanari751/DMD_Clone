using UnityEngine;
using System.Collections;
using DG.Tweening; // DoTween 네임스페이스 추가

public class BasicEnemy : MonoBehaviour, IDamageable
{
    public enum EnemyType
    {
        Melee,
        Arrow
    }

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float retreatSpeed = 1.5f;
    [SerializeField] private float deathTime = 1f; // 사망 애니메이션 지속 시간
    [SerializeField] private float knockbackDistance = 1f; // 넉백 거리
    [SerializeField] private float knockbackDuration = 0.5f; // 넉백 지속 시간
    [SerializeField] private Color knockbackColor = Color.black; // 넉백 시 변경될 색상
    [SerializeField] private float experienceDrop; // 경험치 드랍 양
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private int expOrbCount = 1;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isKnockedBack = false;
    private bool isDead = false;
    private PlayerManager playerManager;
    private ExpOrbPool expOrbPool;
    private Sequence knockbackSequence;

    public event System.Action<GameObject> OnEnemyDeath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        DOTween.SetTweensCapacity(1000, 500);
    }

    private void Start()
    {
        ResetEnemy();
        // PlayerManager 찾기
        playerManager = FindAnyObjectByType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager를 찾을 수 없습니다!");
        }
        expOrbPool = FindAnyObjectByType<ExpOrbPool>();
        if (expOrbPool == null)
        {
            Debug.LogError("ExpOrbPool을 찾을 수 없습니다!");
        }
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    public float GetRetreatSpeed()
    {
        return retreatSpeed;
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        currentHealth -= damage;

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
        else
        {
            ApplyKnockback(knockbackDirection);
        }
        
        if (DamageIndicator.Instance != null)
        {
            DamageIndicator.Instance.ShowDamage(transform.position, Mathf.RoundToInt(damage));
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void ApplyKnockback(Vector2 knockbackDirection)
    {
        isKnockedBack = true;
        Vector2 knockbackTarget = (Vector2)transform.position + knockbackDirection.normalized * knockbackDistance;

        // 이전 시퀀스가 실행 중이라면 중지
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }

        // 새로운 시퀀스 생성
        knockbackSequence = DOTween.Sequence();

        // 스프라이트 색상을 knockbackColor로 변경
        knockbackSequence.Append(spriteRenderer.DOColor(knockbackColor, knockbackDuration * 0.5f));

        // 넉백 이동
        knockbackSequence.Join(transform.DOMove(knockbackTarget, knockbackDuration).SetEase(Ease.OutQuad));

        // 원래 색상으로 복귀
        knockbackSequence.Append(spriteRenderer.DOColor(originalColor, knockbackDuration * 0.5f));

        // 시퀀스 완료 후 넉백 상태 해제 및 시퀀스 정리
        knockbackSequence.OnComplete(() => {
            isKnockedBack = false;
            knockbackSequence = null;
        });
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            FadeOutAndDestroy();
            OnEnemyDeath?.Invoke(gameObject);
            
            SpawnExpOrbs();
        }
    }

    private void SpawnExpOrbs()
    {
        for (int i = 0; i < expOrbCount; i++)
        {
            GameObject expOrb = expOrbPool.GetExpOrb();
            if (expOrb != null)
            {
                expOrb.transform.position = transform.position;
                expOrb.SetActive(true);
                ExpOrb orbScript = expOrb.GetComponent<ExpOrb>();
                if (orbScript != null)
                {
                    orbScript.SetExperience(experienceDrop / expOrbCount);
                }
            }
        }
    }

    private void FadeOutAndDestroy()
    {
        // 기존 색상 저장
        Color startColor = spriteRenderer.color;

        // DOTween을 사용하여 페이드 아웃 애니메이션 실행
        spriteRenderer.DOColor(new Color(startColor.r, startColor.g, startColor.b, 0f), deathTime)
            .OnComplete(() => 
            {
                gameObject.SetActive(false);  // 오브젝트 풀링을 위해 비활성화
            });
    }

    // OnDisable 메서드 추가
    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 모든 DOTween 애니메이션 중지
        DOTween.Kill(spriteRenderer);
        if (knockbackSequence != null && knockbackSequence.IsActive())
        {
            knockbackSequence.Kill();
        }
    }

    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isKnockedBack = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
