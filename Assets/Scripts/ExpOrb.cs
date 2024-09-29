using UnityEngine;

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

    private PlayerManager playerManager;
    private bool isMovingToPlayer = false;
    private float currentMoveSpeed;
    private Vector2 scatterDirection;
    private float scatterDistance;
    private bool isScattering = true;

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    private void OnEnable()
    {
        isMovingToPlayer = false;
        currentMoveSpeed = initialMoveSpeed;
        ScatterOrb();
    }
      private void ScatterOrb()
      {
          scatterDirection = Random.insideUnitCircle.normalized;
          scatterDistance = Random.Range(minScatterDistance, maxScatterDistance);
          isScattering = true;
          currentMoveSpeed = initialMoveSpeed;
      }

      private void Update()
      {
          if (isScattering)
          {
              currentMoveSpeed = Mathf.Min(currentMoveSpeed + accelerationRate * Time.deltaTime, maxMoveSpeed);
              transform.Translate(scatterDirection * currentMoveSpeed * Time.deltaTime);
              scatterDistance -= currentMoveSpeed * Time.deltaTime;
              if (scatterDistance <= 0)
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
}