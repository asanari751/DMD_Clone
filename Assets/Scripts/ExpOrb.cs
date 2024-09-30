using UnityEngine;
using DG.Tweening;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private float magnateArea;
    [SerializeField] private float initialMoveSpeed;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float accelerationRate;
    [SerializeField] private float experienceValue;
    [SerializeField] private float collectionDistance;

    [SerializeField] private float minScatterDistance;
    [SerializeField] private float maxScatterDistance;
    [SerializeField] private float initialScatterSpeed = 5f;
    [SerializeField] private float finalScatterSpeed = 1f;

    private PlayerManager playerManager;
    private bool isMovingToPlayer = false;
    private float currentMoveSpeed;
    private Vector2 scatterDirection;
    private float scatterDistance;
    private bool isScattering = true;
    private float scatterProgress;

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    private void OnEnable()
    {
        isMovingToPlayer = false;
        currentMoveSpeed = initialMoveSpeed;
        Scatter();
    }

    private void Update()
    {
        if (isScattering)
        {
            scatterProgress += Time.deltaTime / scatterDistance;
            float t = Mathf.Clamp01(scatterProgress);
            currentMoveSpeed = Mathf.Lerp(initialScatterSpeed, finalScatterSpeed, t * t);

            transform.Translate(scatterDirection * currentMoveSpeed * Time.deltaTime);

            if (scatterProgress >= 1f)
            {
                isScattering = false;
                currentMoveSpeed = initialMoveSpeed;
            }
        }
        else if (playerManager != null)
        {
            Vector2 playerPosition = playerManager.transform.position;
            float distanceToPlayer = Vector2.Distance(transform.position, playerPosition);

            if (distanceToPlayer <= magnateArea)
            {
                isMovingToPlayer = true;
            }

            if (isMovingToPlayer)
            {
                currentMoveSpeed = Mathf.Min(currentMoveSpeed + accelerationRate * Time.deltaTime, maxMoveSpeed);
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

    private void Scatter()
    {
        scatterDirection = Random.insideUnitCircle.normalized;
        scatterDistance = Random.Range(minScatterDistance, maxScatterDistance);
        scatterProgress = 0f;
        isScattering = true;
    }
}