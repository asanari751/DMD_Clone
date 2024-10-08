using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private bool isRunning = true;
    public static bool Paused { get; private set; } = false;
    private bool isGameEnded;
    private bool tempRunning = true;

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

    public void TempPause()
    {
        tempRunning = false;
        Time.timeScale = 0f;
        Paused = true;
        uiManager.SetPauseUIVisibility(true);
    }

    public void TempResume()
    {
        tempRunning = true;
        Time.timeScale = 1f;
        Paused = false;
        uiManager.SetPauseUIVisibility(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tempRunning)
            {
                TempPause();
            }

            else
            {
                TempResume();
            }
        }
    }

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
