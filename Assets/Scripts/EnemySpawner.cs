using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다.");
            return;
        }
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            BasicEnemy basicEnemy = enemy.GetComponent<BasicEnemy>();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 cameraSize = new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);
        float spawnDistance = 0f; // 카메라 경계선으로부터의 거리

        float randomSide = Random.value;
        if (randomSide < 0.25f) // 위쪽
        {
            return new Vector2(Random.Range(-cameraSize.x, cameraSize.x), cameraSize.y + Random.Range(0f, spawnDistance));
        }
        else if (randomSide < 0.5f) // 오른쪽
        {
            return new Vector2(cameraSize.x + Random.Range(0f, spawnDistance), Random.Range(-cameraSize.y, cameraSize.y));
        }
        else if (randomSide < 0.75f) // 아래쪽
        {
            return new Vector2(Random.Range(-cameraSize.x, cameraSize.x), -cameraSize.y - Random.Range(0f, spawnDistance));
        }
        else // 왼쪽
        {
            return new Vector2(-cameraSize.x - Random.Range(0f, spawnDistance), Random.Range(-cameraSize.y, cameraSize.y));
        }
    }
}
