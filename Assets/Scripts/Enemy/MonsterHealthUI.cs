using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private float yOffset = 1f;
    [SerializeField] private bool isActive = true;

    private GameObject healthBarInstance;
    private Canvas canvas;
    private Camera mainCamera;
    private RectTransform canvasRectTransform;

    private void Start()
    {
        canvas = FindAnyObjectByType<Canvas>();
        mainCamera = Camera.main;
        canvasRectTransform = canvas.GetComponent<RectTransform>();

        healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
    }

    private void Update()
    {
        if (isActive)
        {
            UpdateHealthBarPosition();
        }
    }

    private void UpdateHealthBarPosition()
    {
        if (isActive)
        {
            Vector3 enemyPosition = transform.position;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(new Vector3(enemyPosition.x, enemyPosition.y + yOffset, enemyPosition.z));

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, null, out Vector2 localPoint))
            {
                healthBarInstance.transform.localPosition = localPoint;
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        healthBarInstance.SetActive(active);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        Image fillImage = healthBarInstance.transform.Find("Fill").GetComponent<Image>();
        fillImage.fillAmount = currentHealth / maxHealth;
    }

    public void DestroyHealthBar()
    {
        Destroy(healthBarInstance);
        isActive = false;
    }
}