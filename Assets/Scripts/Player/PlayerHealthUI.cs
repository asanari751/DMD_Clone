using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image healthBackground;
    [SerializeField] private Image healthDelayedImage;
    [SerializeField] private Image healthUIImage;
    [SerializeField] private Image healthUIDelayedImage;
    [SerializeField] private float deathAnimationDuration;
    [SerializeField] private float colorChangeDuration;
    [SerializeField] private float delayedDuration;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float yOffset;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private GameObject playerShadow;

    [Header("Ref")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private string hubScene = "1_Hub";
    [SerializeField] private bool InCombatArea = false;
    private Rigidbody2D playerRigidbody;

    private Vector3 currentPosition;
    private Color originalHealthBarColor;
    private bool isDead = false;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Start()
    {
        playerShadow.SetActive(true);
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (SceneManager.GetActiveScene().name != hubScene)
        {
            healthBarRoot.SetActive(InCombatArea);
            currentHealth = maxHealth;
            UpdateHealthBar(currentHealth, maxHealth);
            originalHealthBarColor = healthFillImage.color;
            healthDelayedImage.fillAmount = healthFillImage.fillAmount;

            // 임시 설정 - 게임 매니저로 분리
            CameraManager.Instance.SetTarget(playerTransform);
        }
        else
        {
            healthBarRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform != null && SceneManager.GetActiveScene().name != hubScene)
        {
            healthBarRoot.SetActive(InCombatArea);
            if (InCombatArea)
            {
                HandleHealthBarPosition();
            }
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            float targetFillAmount = currentHealth / maxHealth;

            // 메인 체력바는 즉시 갱신
            healthFillImage.fillAmount = targetFillAmount;
            healthUIImage.fillAmount = targetFillAmount;

            // 지연 체력바는 천천히 따라오도록 설정
            healthDelayedImage.DOFillAmount(targetFillAmount, delayedDuration)
                .SetEase(Ease.InOutSine);
            healthUIDelayedImage.DOFillAmount(targetFillAmount, delayedDuration)
                .SetEase(Ease.InOutSine);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthBar(currentHealth, maxHealth);
        HandleDamageVisuals();
        audioManager.PlayAmbient("A15");

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ShakeCamera();
        }

        if (currentHealth <= 0)
            Die();
    }

    public void TakeDamageWithKnockback(float damage, Vector2 knockbackDirection, float knockbackForce)
    {
        TakeDamage(damage);
        audioManager.PlayAmbient("A15");

        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ApplyKnockback();
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            // Raycast로 충돌 체크
            RaycastHit2D hit = Physics2D.Raycast(transform.position, knockbackDirection, knockbackForce, collisionMask);
            Vector2 targetPosition;

            if (hit.collider != null)
            {
                // 충돌지점 직전으로 위치 조정 (약간의 여유 공간 추가)
                targetPosition = hit.point - (knockbackDirection * 0.1f);
            }
            else
            {
                // 충돌이 없으면 원래 계획된 위치로
                targetPosition = (Vector2)transform.position + (knockbackDirection * knockbackForce);
            }

            transform.DOMove(targetPosition, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    rb.linearVelocity = Vector2.zero;
                });
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void DisablePlayerComponents()
    {
        var components = new Behaviour[]
        {
        GetComponent<PlayerStateManager>(),
        GetComponent<PlayerController>()
        };

        foreach (var component in components)
        {
            if (component != null)
                component.enabled = false;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Static;
    }

    public void HandleDamageVisuals()
    {
        healthFillImage.DOColor(Color.white, colorChangeDuration).OnComplete(() =>
        {
            healthFillImage.color = originalHealthBarColor;
        });

        healthUIImage.DOColor(Color.white, colorChangeDuration).OnComplete(() =>
        {
            healthUIImage.color = originalHealthBarColor;
        });
    }

    private void HandleHealthBarPosition()
    {
        Vector3 playerPosition = playerTransform.position;
        Vector3 targetPosition = new Vector3(playerPosition.x, playerPosition.y + yOffset, playerPosition.z);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPosition);

        currentPosition = Vector3.Lerp(healthBarRoot.transform.position, screenPosition, Time.deltaTime * smoothSpeed);
        healthBarRoot.transform.position = currentPosition;
    }

    private void Die()
    {
        isDead = true;
        DisablePlayerComponents();
        if (playerShadow != null)
        {
            playerShadow.SetActive(false);
        }

        var animController = GetComponent<AnimationController>();
        if (animController != null)
        {
            animController.PlayDeathAnimation();
            StartCoroutine(ShowResultsAfterAnimation());
        }
    }

    private IEnumerator ShowResultsAfterAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        ResultUIManager resultUI = FindAnyObjectByType<ResultUIManager>();
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        if (resultUI != null)
        {
            uIManager.ShowStageClearUI();
            resultUI.ShowResults();
        }
    }

    private IEnumerator TransitionAfterAnimation(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // 실제 시간 기준으로 대기
        SceneTransitionManager.Instance.LoadSceneWithTransition(hubScene);
    }

    // 패시브 스킬 ( PlayerSkill.cs )

    public void UpdateHealthBarForHeal(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            float targetFillAmount = currentHealth / maxHealth;

            // 회복 시에는 지연 체력바가 즉시 차오름
            healthDelayedImage.fillAmount = targetFillAmount;
            healthUIDelayedImage.fillAmount = targetFillAmount;

            // 메인 체력바가 천천히 따라감
            healthFillImage.DOFillAmount(targetFillAmount, delayedDuration)
                .SetEase(Ease.InOutSine);
            healthUIImage.DOFillAmount(targetFillAmount, delayedDuration)
                .SetEase(Ease.InOutSine);
        }
    }

    // Heal 메서드 수정
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        audioManager.PlayAmbient("A12");
        UpdateHealthBarForHeal(currentHealth, maxHealth);
    }

    // IncreaseMaxHealth 메서드 수정
    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateHealthBarForHeal(currentHealth, maxHealth);
    }
}