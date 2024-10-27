using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private bool InCombatArea = false;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Canvas targetCanvas;
    private float currentHealth;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    private Color originalPlayerColor;
    private Color originalHealthBarColor;

    private GameObject healthBarInstance;
    private Canvas canvas;
    private Image healthFillImage;
    private bool isDead = false;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "1_Hub")
        {
            canvas = targetCanvas;
            healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
            healthBarInstance.SetActive(InCombatArea);
            healthBarInstance.transform.SetAsFirstSibling();

            healthFillImage = healthBarInstance.GetComponentInChildren<Image>();
            currentHealth = maxHealth;
            UpdateHealthBar(currentHealth, maxHealth);

            originalPlayerColor = playerSpriteRenderer.color;
            originalHealthBarColor = healthFillImage.color;
        }
    }

    private void Update()
    {
        if (playerTransform != null && SceneManager.GetActiveScene().name != "1_Hub")
        {
            healthBarInstance.SetActive(InCombatArea);
            if (InCombatArea)
            {
                Vector3 playerPosition = playerTransform.position;
                healthBarInstance.transform.position = Camera.main.WorldToScreenPoint(new Vector3(playerPosition.x, playerPosition.y + yOffset, playerPosition.z));
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthBar(currentHealth, maxHealth);

        // playerSpriteRenderer.DOColor(Color.white, 0.1f).OnComplete(() =>
        // {
        //     playerSpriteRenderer.color = originalPlayerColor;
        // });

        healthFillImage.DOColor(Color.white, 0.1f).OnComplete(() =>
        {
            healthFillImage.color = originalHealthBarColor;
        });

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void Die()
    {
        // 체력바 비활성화
        if (healthBarInstance != null)
        {
            healthBarInstance.SetActive(false);
        }

        // 플레이어 상태 관리자 비활성화
        PlayerStateManager playerStateManager = GetComponent<PlayerStateManager>();
        if (playerStateManager != null)
        {
            playerStateManager.enabled = false;
        }

        // 플레이어 이동 제한
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Rigidbody2D 정적으로 설정
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        // 애니메이션 컨트롤러를 통해 사망 애니메이션 재생
        AnimationController animController = GetComponent<AnimationController>();
        if (animController != null)
        {
            animController.PlayDeathAnimation();
            StartCoroutine(TransitionAfterAnimation(2f)); // 사망 애니메이션 재생 시간을 2초로 고정
        }
        else
        {
            SceneTransitionManager.Instance.LoadSceneWithFade("1_Hub");
        }
    }

    private IEnumerator TransitionAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneTransitionManager.Instance.LoadSceneWithFade("1_Hub");
    }
}