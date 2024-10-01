using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BossHealthUI bossHealthUI;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject eliteEnemyPrefab;
    [SerializeField] private GameObject bossEnemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maxEnemies;

    private Camera mainCamera;
    private List<GameObject> enemyPool;
    private int currentEnemyCount = 0;
    

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        InitializeEnemyPool();
        StartCoroutine(SpawnEnemies());

        if (gameTimerController != null)
        {
            gameTimerController.OnEliteTime += SpawnEliteEnemy;
            gameTimerController.OnBossTime += SpawnBossEnemy;
        }
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
                basicEnemy.ResetEnemy();
                basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;
                basicEnemy.OnEnemyDeath += ReturnEnemyToPool;
            }

            enemy.SetActive(true);
            currentEnemyCount++;
        }
    }

    private void SpawnEliteEnemy()
    {
        GameObject eliteEnemy = SpawnSpecialEnemy(eliteEnemyPrefab);
        if (eliteEnemy != null)
        {
            EliteEnemy eliteEnemyScript = eliteEnemy.GetComponent<EliteEnemy>();
            if (eliteEnemyScript != null)
            {
                eliteEnemyScript.OnEliteEnemyDeath += OnEliteEnemyDeath;
            }
        }
    }

    private void OnEliteEnemyDeath()
    {
        gameTimerController.RemoveCombatAreaLimits();
        uiManager.OnResumeButtonClick();
    }

    private void SpawnBossEnemy()
    {
        GameObject bossEnemy = SpawnSpecialEnemy(bossEnemyPrefab);
        // bossHealthUI.SetActive(true);
        if (bossEnemy != null)
        {
            EnemyBoss bossEnemyScript = bossEnemy.GetComponent<EnemyBoss>();
            if (bossEnemyScript != null)
            {
                bossEnemyScript.OnBossEnemyDeath += OnBossEnemyDeath;
            }
        }
    }

    private void OnBossEnemyDeath()
    {
        gameTimerController.RemoveCombatAreaLimits();
        uiManager.OnResumeButtonClick();
    }

    private GameObject SpawnSpecialEnemy(GameObject specialEnemyPrefab)
    {
        if (specialEnemyPrefab != null)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            return Instantiate(specialEnemyPrefab, spawnPosition, Quaternion.identity);
        }
        return null;
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
            basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;
        }

        enemy.SetActive(false);
        currentEnemyCount--;
    }

    private Vector2 GetRandomSpawnPosition()
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
