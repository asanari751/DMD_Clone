using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxEnemies = 100;

    private Camera mainCamera;
    private List<GameObject> enemyPool;
    private int currentEnemyCount = 0;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다.");
            return;
        }

        InitializeEnemyPool();
        StartCoroutine(SpawnEnemies());
    }

    private void InitializeEnemyPool()
    {
        enemyPool = new List<GameObject>();
        for (int i = 0; i < maxEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = GetEnemyFromPool();
        if (enemy != null)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            enemy.transform.position = spawnPosition;
            
            BasicEnemy basicEnemy = enemy.GetComponent<BasicEnemy>();
            if (basicEnemy != null)
            {
                basicEnemy.ResetEnemy();  // 적 상태 초기화
                basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;  // 이전 이벤트 구독 제거
                basicEnemy.OnEnemyDeath += ReturnEnemyToPool;  // 새로운 이벤트 구독
            }
            else
            {
                Debug.LogError("BasicEnemy component not found on spawned enemy!");
            }

            enemy.SetActive(true);
            currentEnemyCount++;
        }
        else
        {
            Debug.LogWarning("No available enemies in the pool!");
        }
    }

    private GameObject GetEnemyFromPool()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null;
    }

    private void ReturnEnemyToPool(GameObject enemy)
    {
        BasicEnemy basicEnemy = enemy.GetComponent<BasicEnemy>();
        if (basicEnemy != null)
        {
            basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;  // 이벤트 구독 제거
        }

        enemy.SetActive(false);
        currentEnemyCount--;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 cameraSize = new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);
        float spawnDistance = 1f;

        Vector2 cameraPosition = mainCamera.transform.position; // 카메라 위치 가져오기
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
