using UnityEngine;
using TMPro;
using System;

public class GameTimerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private UIManager uiManager;
    [SerializeField][Range(0.1f, 100f)] private float debugParameter = 1f;
    [SerializeField] private float updateInterval;
    [SerializeField] private float timeSinceLastUpdate;

    [Header("Pause Times")]
    [SerializeField] private float elitePauseTime;
    [SerializeField] private float bossPauseTime;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform playerTransform;

    private float elapsedTime;
    private bool isRunning = true;
    public static bool Paused = false;
    private bool isGameEnded = false;
    private float[] pauseTimes;
    private int currentPauseIndex;

    [SerializeField] private float xOffset = 1f; // X 방향 오프셋
    [SerializeField] private float yOffset = 1f; // Y 방향 오프셋
    [SerializeField] private int squareSize = 5; // 반드시 홀수!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public event System.Action OnEliteTime;
    public event System.Action OnBossTime;

    private void Awake()
    {
        // Pause times in seconds
        pauseTimes = new float[] { elitePauseTime, bossPauseTime };
    }

    private void Start()
    {
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (isRunning && !isGameEnded)
        {
            elapsedTime += Time.deltaTime * debugParameter;
            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= updateInterval)
            {
                UpdateTimerDisplay();
                timeSinceLastUpdate = 0f;
            }

            if (currentPauseIndex < pauseTimes.Length && elapsedTime >= pauseTimes[currentPauseIndex])
            {
                UpdateTimerDisplay();
                PauseTimer();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isRunning)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
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
        if (currentPauseIndex == 0) // 엘리트
        {
            OnEliteTime?.Invoke();
            LimitsCombatArea();
            if (uiManager != null)
            {
                uiManager.ShowResumeButton();
            }
        }

        else if (currentPauseIndex == 1) // 보스
        {
            OnBossTime?.Invoke();
            LimitsCombatArea();
            if (uiManager != null)
            {
                uiManager.ShowResumeButton();
            }

            isGameEnded = true;
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

    public void RemoveCombatAreaLimits()
    {
        GameObject[] limitObjects = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obj in limitObjects)
        {
            Destroy(obj);
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
        Paused = true;
        Time.timeScale = 0f;
        uiManager.SetPauseUIVisibility(true);
    }

    public void ResumeGame()
    {
        if (!isGameEnded)
        {
            isRunning = true;
            Paused = false;
            Time.timeScale = 1f;
            uiManager.SetPauseUIVisibility(false);
        }
    }
}