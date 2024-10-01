using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private GameObject bossHealthBarPrefab;
    [SerializeField] private bool isActive = true;

    private Image bossHealthBarInstance;

    private void Start()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        GameObject instance = Instantiate(bossHealthBarPrefab, canvas.transform);
        bossHealthBarInstance = instance.GetComponent<Image>();
        SetActive(isActive);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        bossHealthBarInstance.enabled = isActive;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        Image fillImage = bossHealthBarInstance.GetComponent<Image>();
        fillImage.fillAmount = currentHealth / maxHealth;
    }

    public void DestroyHealthBar()
    {
        Destroy(bossHealthBarInstance);
        isActive = false;
    }
}