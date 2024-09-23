using UnityEngine;
using TMPro;
using System;

public class GameTimerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private UIManager uiManager;
    [SerializeField] [Range(0.1f, 100f)] private float debugParameter = 1f;

    [Header("Pause Times")]
    [SerializeField] private float elitePauseTime = 5f;
    [SerializeField] private float bossPauseTime = 10f;
    [SerializeField] private GameObject prefabToSpawn; // 생성할 프리팹
    [SerializeField] private Transform playerTransform; // 플레이어의 위치

    private float elapsedTime = 0f;
    private bool isRunning = true;
    private bool isGameEnded = false;
    private float[] pauseTimes;
    private int currentPauseIndex = 0;

    [SerializeField] private float xOffset = 1f; // X 방향 오프셋
    [SerializeField] private float yOffset = 1f; // Y 방향 오프셋
    [SerializeField] private int squareSize = 5; // 반드시 홀수!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    private void Awake()
    {
        // Pause times in seconds
        pauseTimes = new float[] { elitePauseTime, bossPauseTime };
    }

    private void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("Timer Text is not assigned to GameTimerController!");
        }
        if (uiManager == null)
        {
            Debug.LogError("UIManager is not assigned to GameTimerController!");
        }
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (isRunning && !isGameEnded)
        {
            elapsedTime += Time.deltaTime * debugParameter;
            if (currentPauseIndex < pauseTimes.Length && elapsedTime >= pauseTimes[currentPauseIndex])
            {
                PauseTimer();
            }
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
            timerText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    private void PauseTimer()
    {
        isRunning = false;
        if (currentPauseIndex == 0) // Elite pause time
        {
            LimitsCombatArea(); // 플레이어 주변에 프리팹 생성
            if (uiManager != null)
            {
                uiManager.ShowResumeButton();
            }
            else
            {
                Debug.LogError("UIManager is null in GameTimerController. Cannot show resume button.");
            }
        }
        else if (currentPauseIndex == 1) // Boss pause time
        {
            isGameEnded = true;
            Debug.Log("Game ended at boss time");
        }
        currentPauseIndex++;
    }

    private void LimitsCombatArea()
    {
        Vector3 playerPosition = playerTransform.position;
        int halfSize = (squareSize - 1) / 2; // 1 = 중앙 타일, 2 = 각 변의 중앙 타일 제외

        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                if (x == -halfSize || x == halfSize || y == -halfSize || y == halfSize)
                {
                    Vector3 spawnPosition = playerPosition + new Vector3(x * xOffset, y * yOffset, 0);
                    Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    public void ResumeTimer()
    {
        if (!isGameEnded)
        {
            isRunning = true;
            Debug.Log("Timer resumed");
        }
    }

    public void PauseGame()
    {
        isRunning = false;
        Time.timeScale = 0f;
        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {
        if (!isGameEnded)
        {
            isRunning = true;
            Time.timeScale = 1f;
            Debug.Log("Game resumed");
        }
    }
}