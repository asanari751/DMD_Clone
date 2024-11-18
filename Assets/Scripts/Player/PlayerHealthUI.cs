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
    [SerializeField] private float yOffset;
    [SerializeField] private float smoothSpeed;

    [Header("Ref")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string hubScene = "1_Hub";
    [SerializeField] private bool InCombatArea = false;

    private float currentHealth;
    private Vector3 currentPosition;
    private Color originalHealthBarColor;
    private bool isDead = false;

    private void Start()
    {
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

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ShakeCamera();
        }

        if (currentHealth <= 0)
            Die();
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

        var animController = GetComponent<AnimationController>();
        if (animController != null)
        {
            animController.PlayDeathAnimation();
            StartCoroutine(TransitionAfterAnimation(deathAnimationDuration));
        }
        else
        {
            SceneTransitionManager.Instance.LoadSceneWithTransition(hubScene);
        }
    }

    private IEnumerator TransitionAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneTransitionManager.Instance.LoadSceneWithTransition(hubScene);
    }
}