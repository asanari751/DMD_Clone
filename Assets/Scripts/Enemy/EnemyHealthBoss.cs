using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BossHealthUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image healthBackground;
    [SerializeField] private Image healthDelayedImage;
    [SerializeField] private float colorChangeDuration;
    [SerializeField] private float delayedDuration;
    
    private Color originalHealthBarColor;
    private bool isActive = true;

    private void Start()
    {
        originalHealthBarColor = healthFillImage.color;
        healthBarRoot.SetActive(true);
        healthDelayedImage.fillAmount = healthFillImage.fillAmount;
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