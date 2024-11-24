using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyHealthElite : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image healthBackground;
    [SerializeField] private Image healthDelayedImage;
    [SerializeField] private float colorChangeDuration;
    [SerializeField] private float delayedDuration;
    [SerializeField] private float yOffset;

    private Color originalHealthBarColor;
    private Camera mainCamera;
    private bool isActive = true;
    private static bool showHealthBar = true;

    private void Start()
    {
        mainCamera = Camera.main;

        if (!showHealthBar)
        {
            healthBarRoot.SetActive(false);
            return;
        }

        if (healthFillImage != null)
        {
            originalHealthBarColor = healthFillImage.color;
            healthDelayedImage.fillAmount = healthFillImage.fillAmount;
        }
            
        healthBarRoot.SetActive(true);
        showHealthBar = PlayerPrefs.GetInt("ShowDamageIndicator", 1) == 1;
    }

    private void Update()
    {
        if (isActive && healthBarRoot != null)
        {
            HandleHealthBarPosition();
        }
    }

    public static void SetHealthBarVisibility(bool visible)
    {
        showHealthBar = visible;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            float targetFillAmount = currentHealth / maxHealth;

            healthFillImage.fillAmount = targetFillAmount;

            healthDelayedImage.DOFillAmount(targetFillAmount, delayedDuration)
                .SetEase(Ease.InOutSine);
        }
    }

    public void HandleDamageVisuals()
    {
        healthFillImage.DOColor(Color.white, colorChangeDuration).OnComplete(() =>
        {
            healthFillImage.color = originalHealthBarColor;
        });
    }

    private void HandleHealthBarPosition()
    {
        Vector3 enemyPosition = transform.position;
        Vector3 healthBarPosition = new Vector3(enemyPosition.x, enemyPosition.y + yOffset, enemyPosition.z);
        healthBarRoot.transform.position = mainCamera.WorldToScreenPoint(healthBarPosition);
    }

    public void SetActive(bool active)
    {
        isActive = active;
        healthBarRoot.SetActive(active);
    }

    public void DestroyHealthBar()
    {
        healthBarRoot.SetActive(false);
        isActive = false;
    }
}