using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class GameTimerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private UIManager uiManager;
    [SerializeField][Range(0.1f, 100f)] private float debugParameter = 1f;
    [SerializeField] private float updateInterval;
    [SerializeField] private float timeSinceLastUpdate;
    [SerializeField] private Transform obstacleHierahy;
    [SerializeField] private PauseController pauseController;
    [SerializeField] private Image timerFillImage;

    [Header("Pause Times")]
    [SerializeField] private float elitePauseTime;
    [SerializeField] private float bossPauseTime;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform playerTransform;

    private float elapsedTime;
    private float[] pauseTimes;
    private float maxTime = 600f;
    private int currentPauseIndex;

    [SerializeField] private float xOffset = 1f; // X 방향 오프셋
    [SerializeField] private float yOffset = 1f; // Y 방향 오프셋
    [SerializeField] private int squareSize = 5; // 반드시 홀수!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public event System.Action OnEliteTime;
    public event System.Action OnBossTime;
    public event System.Action OnBossDefeated;

    private void Awake()
    {
        pauseTimes = new float[] { elitePauseTime, bossPauseTime };
    }

    private void Start()
    {
        UpdateTimerDisplay();
        OnBossDefeated += HandleStageClear;
    }

    private void Update()
    {
        if (!PauseController.Paused && !pauseController.IsGameEnded() && pauseController.IsRunning())
        {
            elapsedTime += Time.deltaTime * debugParameter;
            timeSinceLastUpdate += Time.deltaTime;

            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = Mathf.Clamp01(elapsedTime / maxTime);
            }

            if (timeSinceLastUpdate >= updateInterval)
            {
                UpdateTimerDisplay();
                timeSinceLastUpdate = 0f;
            }

            if (currentPauseIndex < pauseTimes.Length && elapsedTime >= pauseTimes[currentPauseIndex])
            {
                PauseTimer();
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
        pauseController.SetRunning(false);
        elapsedTime = pauseTimes[currentPauseIndex];
        UpdateTimerDisplay();

        if (currentPauseIndex == 0) // 엘리트
        {
            OnEliteTime?.Invoke();
            LimitsCombatArea();
        }
        else if (currentPauseIndex == 1) // 보스
        {
            OnBossTime?.Invoke();
            LimitsCombatArea();
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
                    Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, obstacleHierahy);
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
        if (!pauseController.IsGameEnded())
        {
            bool currentRunningState = pauseController.IsRunning();
            pauseController.SetRunning(!currentRunningState);
        }
    }

    public void TriggerBossDefeated()
    {
        OnBossDefeated?.Invoke();
    }

    private void HandleStageClear()
    {
        pauseController.PauseForGameClear();
        uiManager.ShowStageClearUI();
        RemoveCombatAreaLimits();
    }

    public float GetElapsedTime() // EnemySpawner
    {
        return elapsedTime;
    }
}