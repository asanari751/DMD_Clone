using UnityEngine;
using TMPro;
using System;

public class GameTimerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private UIManager uiManager;
    [SerializeField] [Range(0.1f, 10f)] private float debugParameter = 1f;

    [Header("Pause Times")]
    [SerializeField] private float elitePauseTime = 5f;
    [SerializeField] private float bossPauseTime = 10f;

    private float elapsedTime = 0f;
    private bool isRunning = true;
    private bool isGameEnded = false;
    private float[] pauseTimes;
    private int currentPauseIndex = 0;

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