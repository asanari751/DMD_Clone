using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using Unity.Collections;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;

        [HideInInspector]
        public float spawnProbability; // 적이 스폰될 확률 (전체 중 비율)
        public float spawnTime0;
        public float spawnTime1;

        [HideInInspector]
        public List<GameObject> enemyPool;
    }

    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private UIManager uiManager;

    [Header("Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemySpawnDataList;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maxEnemies;
    [SerializeField] private Transform parent;

    [Header("Special")]
    [SerializeField] private GameObject eliteEnemy;
    [SerializeField] private GameObject bossEnemy;

    private Camera mainCamera;
    private float gameTime; // 현재 게임 시간
    private int currentEnemyCount = 0;
    public delegate void EnemyDefeatedHandler(GameObject enemy);
    public event EnemyDefeatedHandler OnEnemyDefeated;

    private void Awake()
    {
        AdjustSpawnProbabilities();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        InitializeEnemyPools();
        StartCoroutine(SpawnEnemies());

        if (gameTimerController != null)
        {
            gameTimerController.OnEliteTime += SpawnEliteEnemy;
            gameTimerController.OnBossTime += SpawnBossEnemy;
        }

        eliteEnemy.SetActive(false);
        bossEnemy.SetActive(false);
    }

    private void InitializeEnemyPools()
    {
        foreach (var enemyData in enemySpawnDataList)
        {
            enemyData.enemyPool = new List<GameObject>();
            int poolSize = maxEnemies;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyData.enemyPrefab, parent);
                enemy.SetActive(false);
                enemyData.enemyPool.Add(enemy);
            }
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
        // 현재 게임 시간 업데이트
        gameTime = gameTimerController.GetElapsedTime();

        // 현재 시간에 스폰 가능한 적들 필터링
        List<EnemySpawnData> availableEnemies = enemySpawnDataList.FindAll(enemyData =>
            gameTime >= enemyData.spawnTime0 && gameTime <= enemyData.spawnTime1);

        if (availableEnemies.Count == 0)
            return;

        // 스폰 확률에 따른 가중치 합산
        float totalProbability = availableEnemies.Sum(enemyData => enemyData.spawnProbability);
        float randomPoint = Random.value * totalProbability;

        // 가중치에 따라 적 선택
        float currentSum = 0f;
        EnemySpawnData selectedEnemyData = null;
        foreach (var enemyData in availableEnemies)
        {
            currentSum += enemyData.spawnProbability;
            if (randomPoint <= currentSum)
            {
                selectedEnemyData = enemyData;
                break;
            }
        }

        if (selectedEnemyData != null)
        {
            GameObject enemy = GetEnemyFromPool(selectedEnemyData);
            if (enemy != null)
            {
                Vector2 spawnPosition = GetRandomSpawnPosition();
                enemy.transform.position = spawnPosition;

                DOTween.Kill(enemy.transform);

                enemy.SetActive(true);

                // 적의 각종 컴포넌트 초기화
                EnemyAnimationController animController = enemy.GetComponent<EnemyAnimationController>();
                if (animController != null)
                {
                    animController.ResetAnimationState();
                }

                EnemyMovementController movementController = enemy.GetComponent<EnemyMovementController>();
                if (movementController != null)
                {
                    movementController.ResetMovementController();
                }

                EnemyKnockback knockback = enemy.GetComponent<EnemyKnockback>();
                if (knockback != null)
                {
                    knockback.ResetKnockback();
                }

                BasicEnemy basicEnemy = enemy.GetComponent<BasicEnemy>();
                if (basicEnemy != null)
                {
                    basicEnemy.ResetEnemy();
                    basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;
                    basicEnemy.OnEnemyDeath += ReturnEnemyToPool;
                }

                currentEnemyCount++;
            }
        }
    }

    private GameObject GetEnemyFromPool(EnemySpawnData enemyData)
    {
        foreach (GameObject enemy in enemyData.enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy;
            }
        }
        return null; // 풀에 사용 가능한 적이 없을 경우 null 반환
    }

    // 적을 오브젝트 풀로 되돌리기
    private void ReturnEnemyToPool(GameObject enemy)
    {
        BasicEnemy basicEnemy = enemy.GetComponent<BasicEnemy>();
        if (basicEnemy != null)
        {
            basicEnemy.OnEnemyDeath -= ReturnEnemyToPool;
        }

        OnEnemyDefeated?.Invoke(enemy);  // 이벤트 발생

        DOTween.Kill(enemy.transform);
        enemy.SetActive(false);
        currentEnemyCount--;
    }

    /// <summary>
    /// Special
    /// </summary>

    private void SpawnEliteEnemy()
    {
        if (eliteEnemy != null)
        {
            eliteEnemy.transform.position = GetRandomSpawnPosition();
            eliteEnemy.SetActive(true);

            EnemyElite eliteEnemyScript = eliteEnemy.GetComponent<EnemyElite>();
            if (eliteEnemyScript != null)
            {
                eliteEnemyScript.OnEliteEnemyDeath += OnEliteEnemyDeath;
            }
        }
    }

    private void OnEliteEnemyDeath()
    {
        gameTimerController.RemoveCombatAreaLimits();
        gameTimerController.ResumeTimer();
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
                bossEnemyScript.OnBossEnemyDeath += () => StartCoroutine(OnBossEnemyDeath());
            }
        }
    }

    private IEnumerator OnBossEnemyDeath()
    {
        gameTimerController.RemoveCombatAreaLimits();
        yield return new WaitForSeconds(2f);

        gameTimerController.TriggerBossDefeated();
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

    /// <summary>
    /// Special
    /// </summary>

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

    private void AdjustSpawnProbabilities()
    {
        float totalProbability = enemySpawnDataList.Sum(data => data.spawnProbability);
        if (totalProbability != 100f)
        {
            float scale = 100f / totalProbability;
            foreach (var data in enemySpawnDataList)
            {
                data.spawnProbability = Mathf.Round(data.spawnProbability * scale);
            }
        }
    }
}
