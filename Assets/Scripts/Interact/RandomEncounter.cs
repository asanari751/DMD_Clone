using UnityEngine;

public class RandomEncounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] encounterPrefabs;
    [SerializeField] private GameObject player;

    [Header("Encounter Settings")]
    [SerializeField] private float encounterChance; // 기본 10%
    [SerializeField] private float moveThreshold = 1f;
    private Vector3 lastPlayerPosition;
    private Camera mainCamera;

    void Start()
    {
        lastPlayerPosition = player.transform.position;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, lastPlayerPosition) > moveThreshold)
        {
            if (Random.value < encounterChance)
            {
                SpawnRandomEncounter();
            }

            lastPlayerPosition = player.transform.position;
        }
    }

    void SpawnRandomEncounter()
    {
        GameObject prefabToSpawn = encounterPrefabs[Random.Range(0, encounterPrefabs.Length)];
        Vector2 spawnPosition = GetRandomSpawnPosition();
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        int maxAttempts = 3;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 spawnPosition = GenerateRandomPosition();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 0.5f);

            if (colliders.Length == 0)
            {
                return spawnPosition;
            }
        }

        return Vector2.zero;
    }

    private Vector2 GenerateRandomPosition()
    {
        Vector2 cameraSize = new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);
        float spawnDistance = 1f;

        Vector2 cameraPosition = mainCamera.transform.position;
        float randomSide = Random.value;

        if (randomSide < 0.25f) // 위쪽
        {
            return new Vector2(Random.Range(-cameraSize.x, cameraSize.x) + cameraPosition.x, cameraPosition.y + cameraSize.y + spawnDistance);
        }
        else if (randomSide < 0.5f) // 오른쪽
        {
            return new Vector2(cameraPosition.x + cameraSize.x + spawnDistance, Random.Range(-cameraSize.y, cameraSize.y) + cameraPosition.y);
        }
        else if (randomSide < 0.75f) // 아래쪽
        {
            return new Vector2(Random.Range(-cameraSize.x, cameraSize.x) + cameraPosition.x, cameraPosition.y - cameraSize.y - spawnDistance);
        }
        else // 왼쪽
        {
            return new Vector2(cameraPosition.x - cameraSize.x - spawnDistance, Random.Range(-cameraSize.y, cameraSize.y) + cameraPosition.y);
        }
    }
}