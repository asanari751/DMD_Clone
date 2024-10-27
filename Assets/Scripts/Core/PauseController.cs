using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private bool isRunning = true;
    public static bool Paused { get; private set; } = false;
    private bool isGameEnded;
    private bool tempRunning = true;

    public void ToggleUIState()
    {
        tempRunning = !tempRunning;
        Time.timeScale = tempRunning ? 1f : 0f;
        Paused = !tempRunning;
        uiManager.SetPauseUIVisibility(!tempRunning);
    }

    public void PauseForGameClear()
    {
        Paused = true;
        Time.timeScale = 0f;
    }

    public void ResumeForGameClear()
    {
        Paused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tempRunning)
            {
                ToggleUIState();
            }

            else
            {
                ToggleUIState();
            }
        }
    }

    // ===================================================

    public bool IsRunning()
    {
        return isRunning;
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public bool IsGameEnded()
    {
        return isGameEnded;
    }

    public void SetGameEnded(bool ended)
    {
        isGameEnded = ended;
    }
}
