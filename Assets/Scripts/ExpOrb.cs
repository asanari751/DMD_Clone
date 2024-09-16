using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private float magnateArea = 3f;
    [SerializeField] private float initialMoveSpeed = 2f;
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float accelerationRate = 5f;
    [SerializeField] private float experienceValue;
    [SerializeField] private float collectionDistance = 0.5f;

    private PlayerManager playerManager;
    private bool isMovingToPlayer = false;
    private float currentMoveSpeed;

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    private void OnEnable()
    {
        isMovingToPlayer = false;
        currentMoveSpeed = initialMoveSpeed;
    }

    private void Update()
    {
        if (playerManager != null)
        {
            Vector2 playerPosition = playerManager.transform.position;
            float distanceToPlayer = Vector2.Distance(transform.position, playerPosition);
            
            if (distanceToPlayer <= magnateArea)
            {
                isMovingToPlayer = true;
            }

            if (isMovingToPlayer)
            {
                // 가속도 적용
                currentMoveSpeed = Mathf.Min(currentMoveSpeed + accelerationRate * Time.deltaTime, maxMoveSpeed);

                // 플레이어 방향으로 이동
                Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, playerPosition, currentMoveSpeed * Time.deltaTime);

                if (distanceToPlayer <= collectionDistance)
                {
                    CollectExperience();
                }
            }
        }
    }

    private void CollectExperience()
    {
        if (playerManager != null && playerManager.experience != null)
        {
            playerManager.experience.AddExperience(experienceValue);
        }
        gameObject.SetActive(false);
    }

    public void SetExperience(float value)
    {
        experienceValue = value;
    }
}
