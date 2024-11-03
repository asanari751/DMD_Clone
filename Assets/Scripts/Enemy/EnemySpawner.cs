using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BossHealthUI bossHealthUI;

    [Header("Enemys")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private GameObject eliteEnemy;
    [SerializeField] private GameObject bossEnemy;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maxEnemies;
    [SerializeField] private Transform parent;

    private Camera mainCamera;
    private List<List<GameObject>> enemyPool;
    private int currentEnemyCount = 0;
    private int currentEnemyType = 0;


    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        InitializeEnemyPool();
        StartCoroutine(SpawnEnemies());

        if (gameTimerController != null)
        {
            gameTimerController.OnEliteTime += SpawnEliteEnemy;
            gameTimerController.OnBossTime += SpawnBossEnemy;
        }

        eliteEnemy.SetActive(false);
        bossEnemy.SetActive(false);
    }

    private void InitializeEnemyPool()
    {
        enemyPool = new List<List<GameObject>>();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            List<GameObject> typePool = new List<GameObject>();
            for (int j = 0; j < maxEnemies / enemyPrefabs.Count; j++)
            {
                GameObject enemy = Instantiate(enemyPrefabs[i], parent);
                enemy.SetActive(false);
                typePool.Add(enemy);
            }
            enemyPool.Add(typePool);
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
        GameObject enemy = GetEnemyFromPool(currentEnemyType);
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

            DOTween.Kill(this);
            enemy.SetActive(true);

            EnemyAnimationController animController = enemy.GetComponent<EnemyAnimationController>();
            if (animController != null)
            {
                animController.ResetAnimationState();
            }

            currentEnemyCount++;
            currentEnemyType = (currentEnemyType + 1) % enemyPrefabs.Count;
        }
    }

    private void SpawnEliteEnemy()
    {
        if (eliteEnemy != null)
        {
            eliteEnemy.transform.position = GetRandomSpawnPosition();
            eliteEnemy.SetActive(true);

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
    }

    private void SpawnBossEnemy()
    {
        if (bossEnemy != null)
        {
            bossEnemy.transform.position = GetRandomSpawnPosition();
            bossEnemy.SetActive(true);

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

    private GameObject GetEnemyFromPool(int type)
    {
        foreach (GameObject enemy in enemyPool[type])
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

        DOTween.Kill(enemy.transform);

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
