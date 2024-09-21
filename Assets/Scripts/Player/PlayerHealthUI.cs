using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private bool InCombatArea = false; // 전투 중 여부

    private GameObject healthBarInstance;
    private Canvas canvas;

    private void Start()
    {
        canvas = FindAnyObjectByType<Canvas>();
        healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
        healthBarInstance.SetActive(InCombatArea); // 초기 상태 설정
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // InCombatArea가 true일 때만 체력바 표시
            healthBarInstance.SetActive(InCombatArea);
            if (InCombatArea)
            {
                Vector3 playerPosition = playerTransform.position;
                healthBarInstance.transform.position = Camera.main.WorldToScreenPoint(new Vector3(playerPosition.x, playerPosition.y + yOffset, playerPosition.z));
            }
        }
    }
}